using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Core.Fucine.DataImport;
using Boo.Lang;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Noon;
using OrbCreationExtensions;
using UnityEngine;

namespace Assets.Core.Fucine
{
    public class DataProviderForEntityType
    {
        private static readonly string CORE_CONTENT_DIR = Application.streamingAssetsPath + "/content/core/";


        public readonly string EntityFolderName;
        private readonly ContentImportLog _log;
        public ArrayList CoreData { get; set; }
        public Dictionary<string,string> LocalisedValuesData { get; set; }
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
            CoreData = new ArrayList();
            LocalisedValuesData = new Dictionary<string, string>();
        }


        public void LoadEntityData()
        {
            var contentFolder = CORE_CONTENT_DIR + EntityFolderName;

            if (BaseCulture != CurrentCulture)
            {
                GetLocDataForContentType(contentFolder);
            }

            GetCoreDataForContentType(contentFolder);




            var contentImportForMods = new ContentImportForMods();
            contentImportForMods.ProcessContentItemsWithMods(this.CoreData, EntityFolderName);
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

                    JToken topLevelObject = JObject.Parse(json);

                    JArray topLevelArrayList = (JArray)topLevelObject[EntityFolderName];


                    foreach (var eachObject in topLevelArrayList)
                    {
                        UnpackLocalisedObject(eachObject as JObject,string.Empty);
                    }

                }
                catch (Exception e)
                {
                    _log.LogProblem("This file broke: " + contentFile + " with error " + e.Message);
                }


            }

        }

        private void UnpackLocalisedObject(JObject jObject,string currentKey)
        {
            //string nextKey = currentKey+ (string)jObject["id"] + "|".ToLower();
            

            foreach (var eachProperty in jObject)
            {
                EntityUniqueIdBuilder idBuilder = new EntityUniqueIdBuilder(currentKey, (string)jObject[NoonConstants.ID]);
                if (eachProperty.Value.Type == JTokenType.Object)
                 {
                     //string objectKey = $"{nextKey}{{{eachProperty.Key}";
                     idBuilder.WithObjectProperty(eachProperty.Key);

                     //UnpackLocalisedObject(eachProperty.Value as JObject, objectKey);

                     UnpackLocalisedObject(eachProperty.Value as JObject, idBuilder.Key);
                }
                 else if (eachProperty.Value.Type == JTokenType.Array)
                 {


                     foreach (var item in eachProperty.Value)
                     {
                        // UnpackLocalisedObject(item as JObject, nextKey + "[");
                        idBuilder.WithArray();
                        UnpackLocalisedObject(item as JObject, idBuilder.Key);
                    }
                 }

                 else if (eachProperty.Value.Type == JTokenType.String)
                 {
                     if (eachProperty.Key != "id")
                     {

                         idBuilder.WithLeaf(eachProperty.Key);
                        LocalisedValuesData.Add(idBuilder.Key, eachProperty.Value.ToString());
                      //  Debug.Log(idBuilder.Key + ": " + eachProperty.Value.ToString());

                    }
                }


                 else

                 {
                     throw new ApplicationException("Unexpected jtoken type for localised data: " + jObject.Type);
                 }

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
                string json = File.ReadAllText(contentFile);

                try
                {

                  JToken topLevelObject=JObject.Parse(json);

                  JArray topLevelArrayList = (JArray) topLevelObject[EntityFolderName];


                    foreach (var eachObject in topLevelArrayList)
                  {

                      Hashtable currentHashtable = new Hashtable();

                      foreach (var eachKVP in (JObject)eachObject)
                          UnpackTokenToHashtable(eachKVP.Key, currentHashtable, eachKVP.Value);

                      EntityData entityData = new EntityData(currentHashtable);

                      CoreData.Add(entityData);
                  } 
                    
                    
                }
                catch (Exception e)
                {
                    _log.LogProblem("This file broke: " + contentFile + " with error " + e.Message);
                }


            }

        }





        private void UnpackTokenToHashtable(string id, Hashtable currentH, JToken jToken)
        {
            id = id.ToLower();

            if (jToken.Type == JTokenType.String)
            {
                currentH.Add(id, jToken.ToString());
            }

            else if (jToken.Type == JTokenType.Integer)
            {
                currentH.Add(id, (int)jToken);
            }


            else if (jToken.Type == JTokenType.Boolean)
            {
                currentH.Add(id, (bool)jToken);
            }

            else if (jToken.Type == JTokenType.Array)
            {
                var nextList = new ArrayList();
                foreach (var eachItem in (JArray)jToken)
                    UnpackTokenToArrayList(nextList, eachItem);
                currentH.Add(id, nextList);

            }

            else if (jToken.Type == JTokenType.Object)
            {
                var nextH = new Hashtable();
                foreach (var eachKVP in (JObject)jToken)
                    UnpackTokenToHashtable(eachKVP.Key, nextH, eachKVP.Value);

                currentH.Add(id, nextH);

            }

            else
            {
                throw new ApplicationException("Unexpected jtoken type: " + jToken.Type);
            }
        }

        private void UnpackTokenToArrayList(ArrayList currentList, JToken jToken)
        {
            if (jToken.Type == JTokenType.String)
            {
                currentList.Add((string)jToken);
            }

            else if (jToken.Type == JTokenType.Integer)
            {
                currentList.Add((int)jToken);
            }

            else if (jToken.Type == JTokenType.Boolean)
            {
                currentList.Add((bool)jToken);
            }

            else if (jToken.Type == JTokenType.Array)
            {
                var nextList = new ArrayList();
                foreach (var eachItem in (JArray)jToken)
                    UnpackTokenToArrayList(nextList, eachItem);

                currentList.Add(nextList);
            }

            else if (jToken.Type == JTokenType.Object)
            {
                var nextHashtable = new Hashtable();
                foreach (var eachKVP in (JObject)jToken)
                    UnpackTokenToHashtable(eachKVP.Key, nextHashtable, eachKVP.Value);

                currentList.Add(nextHashtable);
            }
            else
            {
                throw new ApplicationException("Unexpected jtoken type: " + jToken.Type);
            }
        }



















    }
}
