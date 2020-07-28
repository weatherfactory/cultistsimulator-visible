using Assets.Core.Fucine.DataImport;
using Newtonsoft.Json.Linq;
using Noon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private List<LoadedContentFile> _contentFiles=new List<LoadedContentFile>();
        public List<EntityData> Entities { get; set; }
        public Dictionary<string,string> LocalisedTextValues { get; set; }
        public string BaseCulture { get; } = NoonConstants.DEFAULT_CULTURE;
        public string CurrentCulture { get; set; }





        public EntityTypeDataLoader(Type entityType,string entityTag, string currentCulture, ContentImportLog log)
        {
            EntityType = entityType;
            EntityTag = entityTag;
            _log = log;
            this.CurrentCulture = currentCulture;
            Entities = new List<EntityData>();
            LocalisedTextValues = new Dictionary<string, string>();
        }


    public void SupplyLoadedContentFiles(IEnumerable<LoadedContentFile> filesToAdd)
        {
            _contentFiles.AddRange(filesToAdd);
        }

        public void LoadCoreData()
        {
            var coreEntitiesLoaded= GetDataForEntityType();
            Entities.AddRange(coreEntitiesLoaded);
        }

        //public void LoadModData()
        //{
        //    //#IN PROGRESS
        //    //get each active mod
        //    //load data from it
        //    //replace as appropriate

        //    var moddedContentFolder = MODS_CONTENT_DIR + EntityFolderName; //this won't work: it needs the subfolder for each mod

        //    var moddedEntitiesLoaded = GetDataForEntityType(moddedContentFolder);

        //    //  var contentImportForMods = new ContentImportForMods();
        //    // contentImportForMods.UpdateEntityDataFromMods(new ArrayList(this.Entities), EntityType);
        //}

      


        private List<EntityData> GetDataForEntityType()
        {
            List<EntityData> entitiesLoaded=new List<EntityData>();
            //load localised data if we're using a non-default culture.
            //We'll use the unique field ids to replace data with localised data further on, if we find matching ids
            //if (BaseCulture != CurrentCulture)
            //{
            //    LoadLocalisedDataForEntityType(locContentFiles);
            //}



            foreach (var contentFile in _contentFiles)
            {

                            var containerBuilder = new FucineUniqueIdBuilder(contentFile.EntityContainer);


                            var topLevelArrayList = (JArray)contentFile.EntityContainer.Value;


                            foreach (var eachObject in topLevelArrayList)
                            {
                                var eachObjectHashtable = new Hashtable();

                                var entityBuilder = new FucineUniqueIdBuilder(eachObject, containerBuilder);


                                foreach (var eachProperty in ((JObject) eachObject).Properties())
                                {
                                    eachObjectHashtable.Add(eachProperty.Name.ToLower(),
                                        UnpackToken(eachProperty, entityBuilder));
                                }

                                var entityData = new EntityData(entityBuilder.UniqueId,eachObjectHashtable);

                                entitiesLoaded.Add(entityData);
                            }
            }

            return entitiesLoaded;
        }


      


        private object UnpackToken(JToken jToken, FucineUniqueIdBuilder tokenIdBuilder)
        {
            if (jToken.Type == JTokenType.Property)
            {
                var propertyBuilder = new FucineUniqueIdBuilder(jToken, tokenIdBuilder);
                return UnpackToken(((JProperty) jToken).Value, propertyBuilder);

            }

            if (jToken.Type == JTokenType.Array)
            {
                var nextList = new ArrayList();
                foreach (var eachItem in (JArray)jToken)
                {
                    var nextBuilder = new FucineUniqueIdBuilder(jToken,tokenIdBuilder);
                    nextList.Add(UnpackToken(eachItem, nextBuilder));
                    
                }

                return nextList;

            }

            else if (jToken.Type == JTokenType.Object)
            {
                //create a hashtable to represent the object
                var subObjectH = new Hashtable();

                var subObjectBuilder = new FucineUniqueIdBuilder(jToken,tokenIdBuilder);


                foreach (var eachKVP in (JObject)jToken)
                {
                    //add each property to that hashtable
                    subObjectH.Add(eachKVP.Key.ToLower(), UnpackToken(eachKVP.Value, subObjectBuilder));
                }

                
                EntityData subEntityData=new EntityData(subObjectBuilder.UniqueId,subObjectH);
                //return the entityData so it can be added in its turn, with the unpacked object
                return subEntityData;
            }

            else
            {


                if (jToken.Type == JTokenType.String)
                {

                    //ASSUMPTION!! Only strings are replaced in loc

                    string uniqueTokenId= new FucineUniqueIdBuilder(jToken, tokenIdBuilder).UniqueId;
                    if (CurrentCulture!=BaseCulture && LocalisedTextValues.TryGetValue(uniqueTokenId, out var localisedString))
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
                    return (bool)jToken;
                }

                else if (jToken.Type == JTokenType.Float)
                {
                    return (double)jToken;
                }
                else
                {
                    throw new ApplicationException("Unexpected jtoken type: " + jToken.Type);
                }
            }
        }

        private void LoadLocalisedDataForEntityType(List<string> locContentFiles)
        {



            foreach (var locContentFile in locContentFiles)
            {
                using (StreamReader file = File.OpenText(locContentFile))
                using (JsonTextReader reader = new JsonTextReader(file))
                {

                    try
                    {

                        var topLevelObject = (JObject)JToken.ReadFrom(reader);
                        var containerProperty =
                            topLevelObject.Properties()
                                .First(); //there should be exactly one property, which contains all the relevant entities
                        //check that the entity in the file matches the tag; if it doesn't, skip this file
                        //This is probably pretty inefficient, so TODO rework
                        //also this arrowhead is probably good to refactor with the core entity loading
                        if (containerProperty.Name.ToLowerInvariant() == EntityTag.ToLowerInvariant())
                        {
                            var containerBuilder = new FucineUniqueIdBuilder(containerProperty);

                            var topLevelArrayList = (JArray) topLevelObject[EntityTag];


                            foreach (var eachObject in topLevelArrayList)
                            {
                                var entityBuilder = new FucineUniqueIdBuilder(eachObject, containerBuilder);

                                foreach (var eachProperty in ((JObject) eachObject).Properties())
                                {
                                    var propertyBuilder = new FucineUniqueIdBuilder(eachProperty, entityBuilder);

                                    RegisterLocalisedValues(eachProperty.Value, propertyBuilder);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _log.LogProblem("This file broke: " + locContentFile + " with error " + e.Message);
                    }
                }

            }

        }

        private void RegisterLocalisedValues(JToken jtoken, FucineUniqueIdBuilder nameBuilder)
        {


            if (jtoken.Type == JTokenType.Object)
            {

                FucineUniqueIdBuilder subObjectBuilder = new FucineUniqueIdBuilder(jtoken, nameBuilder);

                foreach (JProperty jProperty in ((JObject)jtoken).Properties())
                {
                    var subPropertyBuilder = new FucineUniqueIdBuilder(jProperty, subObjectBuilder);
                    RegisterLocalisedValues(jProperty.Value, subPropertyBuilder);

                }
            }

            else if (jtoken.Type == JTokenType.Array)
            {

                FucineUniqueIdBuilder arrayBuilder = new FucineUniqueIdBuilder(jtoken, nameBuilder);

                foreach (var item in ((JArray)jtoken))
                {
                    RegisterLocalisedValues(item, arrayBuilder);
                }
            }

            else if (jtoken.Type == JTokenType.String)
            {
                FucineUniqueIdBuilder builder = new FucineUniqueIdBuilder(jtoken, nameBuilder);

                LocalisedTextValues.Add(builder.UniqueId, ((string)jtoken));

            }

            else

            {
                throw new ApplicationException("Unexpected jtoken type for localised data: " + jtoken.Type);
            }

        }



















    }
}
