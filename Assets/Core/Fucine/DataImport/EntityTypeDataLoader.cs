using Assets.Core.Fucine.DataImport;
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
        private static readonly string CORE_CONTENT_DIR = Application.streamingAssetsPath + "/content/core/";

        private static readonly string MODS_CONTENT_DIR = Path.Combine(Application.persistentDataPath, "mods");

        private const string MOD_MANIFEST_FILE_NAME = "manifest.json";

        private static readonly string ModEnabledListPath = Path.Combine(Application.persistentDataPath, "mods.txt");


        public readonly Type EntityType;
        public readonly string EntityTag;
        private readonly ContentImportLog _log;
        private List<LoadedContentFile> _coreContentFiles = new List<LoadedContentFile>();
        private List<LoadedContentFile> _locContentFiles = new List<LoadedContentFile>();
        private List<LoadedContentFile> _modContentFiles = new List<LoadedContentFile>();
        public List<EntityData> Entities { get; set; }

        /// <summary>
        /// k uniqueid, v original string from file
        /// </summary>
        public Dictionary<string, string> LocalisedTextValuesRegistry { get; set; }

        /// <summary>
        /// k uniqueid, v original string from file
        /// </summary>
        public Dictionary<string, string> ModdedValuesRegistry { get; set; }

        public string BaseCulture { get; } = NoonConstants.DEFAULT_CULTURE;
        public string CurrentCulture { get; set; }


        public EntityTypeDataLoader(Type entityType, string entityTag, string currentCulture, ContentImportLog log)
        {
            EntityType = entityType;
            EntityTag = entityTag;
            _log = log;
            this.CurrentCulture = currentCulture;
            Entities = new List<EntityData>();
            LocalisedTextValuesRegistry = new Dictionary<string, string>();
            ModdedValuesRegistry = new Dictionary<string, string>();
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

        public void LoadDataFromSuppliedFiles()
        {
            var coreEntitiesLoaded = GetDataForEntityType();
            Entities.AddRange(coreEntitiesLoaded);
        }


        private List<EntityData> GetDataForEntityType()
        {
            //load localised data if we're using a non-default culture.
            //We'll use the unique field ids to replace data with localised data down in UnpackToken, if we find matching ids
            if (BaseCulture != CurrentCulture && _locContentFiles.Any())
            {
                RegisterLocalisedDataForEmendations();
            }

            if (_modContentFiles.Any())
            {
                RegisterModDataForEmendations();
            }

            var loadedEntityData = LoadAndReplaceCoreData();
            return loadedEntityData;
        }


        private List<EntityData> LoadAndReplaceCoreData()
        {
            List<EntityData> allEntityData = new List<EntityData>();

            foreach (var contentFile in _coreContentFiles)
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
                            UnpackAndEmendToken(eachProperty, entityBuilder));
                    }

                    //add the just loaded entity data to the list of all entity data, along with its unique id
                    var thisEntityDataItem = new EntityData(entityBuilder.UniqueId, eachObjectHashtable);

                    allEntityData.Add(thisEntityDataItem);
                }
            }

            return allEntityData;
        }

        /// <summary>
        /// Unpack the tokens into individual entity data all the way round, and also replace data that's been localised or modded.
        /// </summary>
        /// <returns></returns>
        private object UnpackAndEmendToken(JToken jToken, FucineUniqueIdBuilder tokenIdBuilder)
        {
            if (jToken.Type == JTokenType.Property)
            {
                var propertyBuilder = new FucineUniqueIdBuilder(jToken, tokenIdBuilder);
                return UnpackAndEmendToken(((JProperty) jToken).Value, propertyBuilder);
            }

            if (jToken.Type == JTokenType.Array)
            {
                var nextList = new ArrayList();
                foreach (var eachItem in (JArray) jToken)
                {
                    var nextBuilder = new FucineUniqueIdBuilder(jToken, tokenIdBuilder);
                    nextList.Add(UnpackAndEmendToken(eachItem, nextBuilder));
                }

                return nextList;
            }

            else if (jToken.Type == JTokenType.Object)
            {
                //create a hashtable to represent the object
                var subObjectH = new Hashtable();

                var subObjectBuilder = new FucineUniqueIdBuilder(jToken, tokenIdBuilder);


                foreach (var eachKVP in (JObject) jToken)
                {
                    //add each property to that hashtable
                    subObjectH.Add(eachKVP.Key.ToLower(), UnpackAndEmendToken(eachKVP.Value, subObjectBuilder));
                }


                EntityData subEntityData = new EntityData(subObjectBuilder.UniqueId, subObjectH);
                //return the entityData so it can be added in its turn, with the unpacked object
                return subEntityData;
            }

            else
            {
                if (jToken.Type == JTokenType.String)
                {
                    //ASSUMPTION!! Only strings are replaced in loc

                    string uniqueTokenId = new FucineUniqueIdBuilder(jToken, tokenIdBuilder).UniqueId;
                    if (CurrentCulture != BaseCulture &&
                        LocalisedTextValuesRegistry.TryGetValue(uniqueTokenId, out var localisedString))
                        return localisedString;
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
                        var propertyIdBuilder = new FucineUniqueIdBuilder(eachProperty, entityBuilder);

                        RegisterEmendationValues(eachProperty.Value, propertyIdBuilder, locContentFile,LocalisedTextValuesRegistry);
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
                _log.LogProblem(
                    $"Unexpected jtoken type in {currentContentFile.Path}: {jtoken.Type}");
            }
        }


        private void RegisterModDataForEmendations()
        {
            foreach (var modContentFile in _modContentFiles)
            {
                var containerBuilder = new FucineUniqueIdBuilder(modContentFile.EntityContainer);
                var topLevelArrayList = (JArray) modContentFile.EntityContainer.Value;
                foreach (var eachObject in topLevelArrayList)
                {
                    var entityIdBuilder = new FucineUniqueIdBuilder(eachObject, containerBuilder);
                    foreach (var eachProperty in ((JObject) eachObject).Properties())
                    {
                        var propertyIdBuilder=new FucineUniqueIdBuilder(eachProperty,entityIdBuilder);
                        RegisterEmendationValues(eachProperty.Value, propertyIdBuilder, modContentFile,ModdedValuesRegistry);
                    }
                }
            }
        }


    }
}