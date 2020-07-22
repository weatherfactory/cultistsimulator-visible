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
            return;
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

                    JToken fileLevelObject = JObject.Parse(json);

                    JArray arrayOfEntitiesOfType = (JArray)fileLevelObject[EntityFolderName];

                
                    foreach (var eachObject in arrayOfEntitiesOfType)
                    {
                        var idBuilder = new EntityUniqueIdBuilder(String.Empty);
                        idBuilder.WithObjectProperty(EntityFolderName, (string)eachObject[NoonConstants.ID]);
                        UnpackLocalisedObject(eachObject as JObject, idBuilder.BuiltId);
                    }

                }
                catch (Exception e)
                {
                    _log.LogProblem("This file broke: " + contentFile + " with error " + e.Message);
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

                      Hashtable eachObjectHashtable = new Hashtable();
                      EntityUniqueIdBuilder idBuilder = new EntityUniqueIdBuilder(String.Empty);
                      idBuilder.WithObjectProperty(EntityFolderName, (string)eachObject[NoonConstants.ID]);

                        foreach (var eachToken in (JObject)eachObject)
                            eachObjectHashtable.Add(eachToken.Key, UnpackToken(eachToken.Value,idBuilder));
//

                      EntityData entityData = new EntityData(eachObjectHashtable);

                      CoreData.Add(entityData);
                  } 
                    
                    
                }
                catch (Exception e)
                {
                    _log.LogProblem("This file broke: " + contentFile + " with error " + e.Message);
                }


            }

        }


        private void UnpackLocalisedObject(JObject jObject, string currentUniqueKey)
        {
           foreach (var eachProperty in jObject)
            {
         
                if (eachProperty.Value.Type == JTokenType.Object)
                {
                    var idBuilder = new EntityUniqueIdBuilder(currentUniqueKey);
                    idBuilder.WithObjectProperty(eachProperty.Key, (string)jObject[NoonConstants.ID]);

                    UnpackLocalisedObject(eachProperty.Value as JObject, idBuilder.BuiltId);
                }
                else if (eachProperty.Value.Type == JTokenType.Array)
                {
                    foreach (var item in eachProperty.Value)
                    {
                        var idBuilder = new EntityUniqueIdBuilder(currentUniqueKey);
                        idBuilder.WithArray(eachProperty.Key);
                        UnpackLocalisedObject(item as JObject, idBuilder.BuiltId);
                    }
                }

                else if (eachProperty.Value.Type == JTokenType.String)
                {
                        var idBuilder = new EntityUniqueIdBuilder(currentUniqueKey);
                        idBuilder.WithLeaf(eachProperty.Key);
                      //  LocalisedValuesData.Add(idBuilder.BuiltId, eachProperty.Value.ToString());
                        NoonUtility.Log(idBuilder.BuiltId + ": " + eachProperty.Value);
                }


                else

                {
                    throw new ApplicationException("Unexpected jtoken type for localised data: " + jObject.Type);
                }
            }
        }


        private object UnpackToken(JToken jToken, EntityUniqueIdBuilder idBuilder)
        {



            if (jToken.Type == JTokenType.Array)
            {
                var nextList = new ArrayList();
                foreach (var eachItem in (JArray)jToken)
                    nextList.Add(UnpackToken(eachItem, idBuilder));

                return nextList;

            }

            else if (jToken.Type == JTokenType.Object)
            {
                var nextH = new Hashtable();

                var thisBuilder = new EntityUniqueIdBuilder(idBuilder);

             //   thisBuilder.WithObjectProperty(id, (string)jToken[NoonConstants.ID]); ;

                foreach (var eachKVP in (JObject)jToken)
                {
                    nextH.Add(eachKVP.Key.ToLower(), UnpackToken(eachKVP.Value,idBuilder)); //SOME SORT OF ISSUE AROUND THIS I CAN'T WORK OUT
                }

                return nextH;

            }

            else
            {
                if (jToken.Type == JTokenType.String)
                {
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
