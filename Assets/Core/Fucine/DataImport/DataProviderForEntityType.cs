using Assets.Core.Fucine.DataImport;
using Newtonsoft.Json.Linq;
using Noon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Core.Fucine
{
    public class DataProviderForEntityType
    {
        private static readonly string CORE_CONTENT_DIR = Application.streamingAssetsPath + "/content/core/";


        public readonly string EntityFolderName;
        private readonly ContentImportLog _log;
        public List<EntityData> Entities { get; set; }
        public Dictionary<string,string> LocalisedTextValues { get; set; }
        public string BaseCulture { get; } = "en";
        public string CurrentCulture { get; set; }


        public string GetBaseFolderForLocalisedData()
        {
            return "core_" + CurrentCulture;
        }


        public DataProviderForEntityType(string entityFolderName, string currentCulture, ContentImportLog log)
        {
            EntityFolderName = entityFolderName; //
            _log = log;
            this.CurrentCulture = currentCulture;
            Entities = new List<EntityData>();
            LocalisedTextValues = new Dictionary<string, string>();
        }


        public void LoadEntityDataFromJson()
        {
            var contentFolder = CORE_CONTENT_DIR + EntityFolderName;

            if (BaseCulture != CurrentCulture)
            {
                GetLocDataForContentType(contentFolder);
            }

            GetCoreDataForContentType(contentFolder);




            var contentImportForMods = new ContentImportForMods();
            contentImportForMods.ProcessContentItemsWithMods(new ArrayList(this.Entities), EntityFolderName);
        }

        private void GetLocDataForContentType(string contentFolder)
        {
     
            string locFolder = contentFolder.Replace("core","core_" + LanguageTable.targetCulture);

            var locContentFiles = Directory.GetFiles(locFolder).ToList().FindAll(f => f.EndsWith(".json"));
            if (locContentFiles.Any())
                locContentFiles.Sort();


            foreach (var contentFile in locContentFiles)
            {
                //json string for each content file - in English initially
                string json = File.ReadAllText(contentFile);

                try
                {

                    var topLevelObject = JObject.Parse(json);
                    var containerProperty =
                        topLevelObject.Properties()
                            .First(); //there should be exactly one property, which contains all the relevant entities
                    var containerBuilder = new FucineUniqueIdBuilder(containerProperty);

                    var topLevelArrayList = (JArray) topLevelObject[EntityFolderName];


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
                catch (Exception e)
                {
                    _log.LogProblem("This file broke: " + contentFile + " with error " + e.Message);
                }


            }

        }

        private void RegisterLocalisedValues(JToken jtoken, FucineUniqueIdBuilder nameBuilder)
        {
  

            if (jtoken.Type == JTokenType.Object) {

                FucineUniqueIdBuilder subObjectBuilder = new FucineUniqueIdBuilder(jtoken, nameBuilder);

                foreach (JProperty jProperty in ((JObject) jtoken).Properties())
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

            else if(jtoken.Type == JTokenType.String)
            {
                FucineUniqueIdBuilder builder = new FucineUniqueIdBuilder(jtoken, nameBuilder);

              LocalisedTextValues.Add(builder.UniqueId, ((string)jtoken));

            }

            else

            {
                throw new ApplicationException("Unexpected jtoken type for localised data: " + jtoken.Type);
            }
        
        }


        public void GetCoreDataForContentType(string contentFolder)
        {
            var coreContentFiles = Directory.GetFiles(contentFolder).ToList().FindAll(f => f.EndsWith(".json"));
            if (coreContentFiles.Any())
                coreContentFiles.Sort();
            else
                _log.LogProblem("Can't find any " + EntityFolderName + " to import as content");


            foreach (var contentFile in coreContentFiles)
            {
                //json string for each content file - in English initially
                var json = File.ReadAllText(contentFile);

                try
                {
                    var topLevelObject = JObject.Parse(json);
                    var containerProperty =
                        topLevelObject.Properties()
                            .First(); //there should be exactly one property, which contains all the relevant entities
                    var containerBuilder = new FucineUniqueIdBuilder(containerProperty);


                    var topLevelArrayList = (JArray) topLevelObject[EntityFolderName];


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

                        Entities.Add(entityData);
                    }
                }
                catch (Exception e)
                {
                    _log.LogProblem("This file broke: " + contentFile + " with error " + e.Message);
                }
            }
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





















    }
}
