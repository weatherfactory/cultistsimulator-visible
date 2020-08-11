﻿using Assets.Core.Fucine.DataImport;
using Newtonsoft.Json.Linq;
using Noon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Core.Fucine

{
    public class EntityTypeDataLoader
    {
        public readonly Type EntityType;
        public readonly string EntityTag;
        public string BaseCulture { get; } = NoonConstants.DEFAULT_CULTURE;
        public string CurrentCulture { get; set; }


        private readonly ContentImportLog _log;
        private List<LoadedContentFile> _coreContentFiles = new List<LoadedContentFile>();
        private List<LoadedContentFile> _locContentFiles = new List<LoadedContentFile>();
        private List<LoadedContentFile> _modContentFiles = new List<LoadedContentFile>();
        private Dictionary<string,EntityData> _allLoadedEntities { get; set; }

        /// <summary>
        /// k uniqueid, v original string from file
        /// </summary>
        private Dictionary<string, string> _localisedTextValuesRegistry { get; set; }

        //TODO: get these from Type
        private HashSet<string> localizableKeys;



        public EntityTypeDataLoader(Type entityType, string entityTag, string currentCulture, ContentImportLog log)
        {
            EntityType = entityType;
            EntityTag = entityTag;
            _log = log;
            this.CurrentCulture = currentCulture;
            _allLoadedEntities = new Dictionary<string, EntityData>();
            _localisedTextValuesRegistry = new Dictionary<string, string>();
        }

        /// <summary>
        /// This can handle being called more than once, although it won't check uniqueness
        /// </summary>
        public void SupplyContentFiles(IEnumerable<LoadedContentFile> coreContentFiles,
            IEnumerable<LoadedContentFile> locContentFiles, IEnumerable<LoadedContentFile> modContentFiles)
        {
            _coreContentFiles.AddRange(coreContentFiles);
            _locContentFiles.AddRange(locContentFiles);
            _modContentFiles.AddRange(modContentFiles);
        }

        public void LoadEntityDataFromSuppliedFiles()
        {
            //load localised data if we're using a non-default culture.
            //We'll use the unique field ids to replace data with localised data down in UnpackToken, if we find matching ids

            if (BaseCulture != CurrentCulture && _locContentFiles.Any())
            {
                localizableKeys = new HashSet<string>();

                var entityTypeProperties = EntityType.GetProperties();

                foreach (var thisProperty in entityTypeProperties)
                {
                    //Note: this doesn't work quite in the way I intended it. Label and Description on Slots are marked as Localise, but
                    //the attribute is inspected only for the top level entity. Because the top level entity also has Label and Description marked as localie,
                    //the slot properties are added to the localisable keys, but this will break if the names are different. Consider explicitly inspecting subproperty attributes
                    //to see if they're also subentities, when loading the data
                    if (Attribute.GetCustomAttribute(thisProperty, typeof(Fucine)) is Fucine fucineAttribute)
                    {
                        if (fucineAttribute.Localise)
                            localizableKeys.Add(thisProperty.Name.ToLower());
                    }
                }

         
                RegisterLocalisedDataForEmendations();
            }

            var coreEntityData = UnpackAndLocaliseData(_coreContentFiles);

            if (_modContentFiles.Any())
            {
                var moddedEntityData = UnpackAndLocaliseData(_modContentFiles);
                //any localisation will also apply to the mod string values
                ApplyModsToCoreData(coreEntityData, moddedEntityData);
            }

            _allLoadedEntities = coreEntityData;
        }

        public List<EntityData> GetLoadedEntityDataAsList()
        {
            return _allLoadedEntities.Values.ToList();
        }


 

        private void ApplyModsToCoreData(Dictionary<string,EntityData> coreEntityData, Dictionary<string, EntityData> moddedEntityData)
        {
            foreach(var modData in moddedEntityData.Values)
            {
                EntityMod entityMod=new EntityMod(modData);
                entityMod.ApplyModTo(coreEntityData,_log);
            }

        }

        private Dictionary<string,EntityData> UnpackAndLocaliseData(List<LoadedContentFile> contentFilesToUnpack)
        {
            Dictionary<string, EntityData> entityDataCollection = new Dictionary<string, EntityData>();

            foreach (var contentFile in contentFilesToUnpack)
            {
                var containerBuilder = new FucineUniqueIdBuilder(contentFile.EntityContainer);


                var topLevelArrayList = (JArray) contentFile.EntityContainer.Value;


                foreach (var eachObject in topLevelArrayList)
                {
                    var eachObjectHashtable = new Hashtable(); //eg Work verb

                    var entityBuilder = new FucineUniqueIdBuilder(eachObject, containerBuilder);


                    foreach (var eachProperty in ((JObject) eachObject).Properties()
                    ) //eg description, but also eg slots - this is why we need to unpack
                    {
                        eachObjectHashtable.Add(eachProperty.Name.ToLower(),
                            UnpackAndLocaliseToken(eachProperty, entityBuilder));
                    }

                    //add the just loaded entity data to the list of all entity data, along with its unique id
                    var thisEntityDataItem = new EntityData(entityBuilder.UniqueId, eachObjectHashtable);

                    entityDataCollection.Add(thisEntityDataItem.Id,thisEntityDataItem);
                }
            }

            return entityDataCollection;
        }

        /// <summary>
        /// Unpack the tokens into individual entity data all the way round, and also replace data that's been localised or modded.
        /// </summary>
        /// <returns></returns>
        private object UnpackAndLocaliseToken(JToken jToken, FucineUniqueIdBuilder tokenIdBuilder)
        {
            if (jToken.Type == JTokenType.Property)
            {
                var propertyBuilder = new FucineUniqueIdBuilder(jToken, tokenIdBuilder);
                return UnpackAndLocaliseToken(((JProperty) jToken).Value, propertyBuilder);
            }

            if (jToken.Type == JTokenType.Array)
            {
                var nextList = new ArrayList();
                foreach (var eachItem in (JArray) jToken)
                {
                    var nextBuilder = new FucineUniqueIdBuilder(jToken, tokenIdBuilder);
                    nextList.Add(UnpackAndLocaliseToken(eachItem, nextBuilder));
                }

                return nextList;
            }

            else if (jToken.Type == JTokenType.Object)
            {
         
                //create a hashtable to represent the object
                var subObjectH = new Hashtable();

                var subObjectBuilder = new FucineUniqueIdBuilder(jToken, tokenIdBuilder);


                foreach (var subProperty in ((JObject) jToken).Properties())
                {
                    var subPropertyIdBuilder=new FucineUniqueIdBuilder(subProperty,subObjectBuilder);
                    //add each property to that hashtable
                    subObjectH.Add(subProperty.Name.ToLower(), UnpackAndLocaliseToken(subProperty.Value, subPropertyIdBuilder));
                }


                EntityData subEntityData = new EntityData(subObjectBuilder.UniqueId, subObjectH);
                //return the entityData so it can be added in its turn, with the unpacked object
                return subEntityData;
            }

            else
            {
                if (jToken.Type == JTokenType.String)
                {


                    string uniqueTokenId = new FucineUniqueIdBuilder(jToken, tokenIdBuilder).UniqueId;
                    if(CurrentCulture != BaseCulture)
                        return TryReplaceWithLocalisedString(jToken, uniqueTokenId);
                    else
                       return jToken.ToString();
                    
                }

                else if (jToken.Type == JTokenType.Integer)
                {
                    return (int) jToken;
                }


                else if (jToken.Type == JTokenType.Boolean)
                {
                    return (bool) jToken;
                }

                else if (jToken.Type == JTokenType.Float)
                {
                    return (double) jToken;
                }
                else
                {
                    throw new ApplicationException("Unexpected jtoken type: " + jToken.Type);
                }
            }
        }

        private object TryReplaceWithLocalisedString(JToken jToken, string uniqueTokenId)
        {
            if (_localisedTextValuesRegistry.TryGetValue(uniqueTokenId, out var localisedString))
                return localisedString;
            else
                return jToken.ToString();
        }

        private void RegisterLocalisedDataForEmendations()
        {
            foreach (var locContentFile in _locContentFiles)
            {
                var containerBuilder = new FucineUniqueIdBuilder(locContentFile.EntityContainer);

                var topLevelArrayList = (JArray) locContentFile.EntityContainer.Value;


                foreach (var eachObject in topLevelArrayList)
                {
                    var entityBuilder = new FucineUniqueIdBuilder(eachObject, containerBuilder);

                    foreach (var eachProperty in ((JObject) eachObject).Properties())
                    {
                        if(localizableKeys.Contains(eachProperty.Name))
                        {
                            var propertyIdBuilder = new FucineUniqueIdBuilder(eachProperty, entityBuilder);
                            RegisterEmendationValues(eachProperty.Value, propertyIdBuilder, locContentFile,_localisedTextValuesRegistry);
                        }
                    }
                }
            }
        }

        private void RegisterEmendationValues(JToken jtoken, FucineUniqueIdBuilder nameBuilder,
            LoadedContentFile currentContentFile,Dictionary<string,string> valuesRegistry)
        {
            if (jtoken.Type == JTokenType.Object)
            {
                FucineUniqueIdBuilder subObjectBuilder = new FucineUniqueIdBuilder(jtoken, nameBuilder);

                foreach (JProperty jProperty in ((JObject) jtoken).Properties())
                {
                    var subPropertyBuilder = new FucineUniqueIdBuilder(jProperty, subObjectBuilder);
                    RegisterEmendationValues(jProperty.Value, subPropertyBuilder, currentContentFile, valuesRegistry);
                }
            }

            else if (jtoken.Type == JTokenType.Array)
            {
                FucineUniqueIdBuilder arrayBuilder = new FucineUniqueIdBuilder(jtoken, nameBuilder);

                foreach (var item in ((JArray) jtoken))
                {
                    RegisterEmendationValues(item, arrayBuilder, currentContentFile, valuesRegistry);
                }
            }

            else if (jtoken.Type == JTokenType.String)
            {
                FucineUniqueIdBuilder builder = new FucineUniqueIdBuilder(jtoken, nameBuilder);
                valuesRegistry.Add(builder.UniqueId, ((string) jtoken));
            }

            else

            {
                    NoonUtility.Log($"Unexpected jtoken type in {currentContentFile.Path}: {jtoken.Type}", 0, VerbosityLevel.SystemChatter);
            }
        }



    }
}