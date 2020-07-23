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

                        UnpackLocalisedObject(eachObject as JObject, new EntityUniqueIdBuilder(eachObject));
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

                    JObject topLevelObject =JObject.Parse(json);
                    JProperty containerProperty = topLevelObject.Properties().First(); //there should be exactly one property, which contains all the relevant entities
                  EntityUniqueIdBuilder containerBuilder = new EntityUniqueIdBuilder(containerProperty);


                    JArray topLevelArrayList = (JArray) topLevelObject[EntityFolderName];
                  
                    
                    foreach (var eachObject in topLevelArrayList)
                  {
                      Hashtable eachObjectHashtable = new Hashtable();

                        EntityUniqueIdBuilder entityBuilder = new EntityUniqueIdBuilder(eachObject, containerBuilder);

                       

                      foreach (var eachProperty in ((JObject) eachObject).Properties())
                      {

                            EntityUniqueIdBuilder propertyBuilder = new EntityUniqueIdBuilder(eachProperty, entityBuilder);

                            NoonUtility.Log(propertyBuilder.UniqueId);

                            eachObjectHashtable.Add(eachProperty.Name.ToLower(), UnpackToken(eachProperty.Value, propertyBuilder));

     

                            //the fundamental problem is still: we want to refer to entities by their id, not their index.
                            //if we get the id, we can use that as the referrer in a path.
                            //BUT it's not actually the referrer in a path. We don't use ID as the key in a hashtable: we have a series of arrays in which
                            //ID is used internally as a property. This is really the whole problem with the whole thing, but I don't want to change it now.
                            //or to put it another way, we sometimes move from 
                            //[{id:"foo",anotherproperty:3}]
                            //to
                            //{"foo":{anotherproperty:3}
                            //however, we do need the ID from each previous stage, too.
                            //We could get that via Parent, but then we might as well pass down the IDbuilder

                        }

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


        private void UnpackLocalisedObject(JObject jObject, EntityUniqueIdBuilder idBuilder)
        {
           foreach (var eachProperty in jObject)
           {
         
               if (eachProperty.Value.Type == JTokenType.Object)
               {

                   UnpackLocalisedObject(eachProperty.Value as JObject, idBuilder);
               }
               else if (eachProperty.Value.Type == JTokenType.Array)
               {
                   foreach (var item in eachProperty.Value)
                   {
                       UnpackLocalisedObject(item as JObject, idBuilder);
                   }
               }

               else if (eachProperty.Value.Type == JTokenType.String)
               {
                   LocalisedValuesData.Add(idBuilder.UniqueId, eachProperty.Value.ToString());
                        
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
                {
                    var nextBuilder = new EntityUniqueIdBuilder(jToken,idBuilder);
                    nextList.Add(UnpackToken(eachItem, nextBuilder));
                    
                }

                return nextList;

            }

            else if (jToken.Type == JTokenType.Object)
            {
                //create a hashtable to represent the object
                var subObjectH = new Hashtable();

                var subObjectBuilder = new EntityUniqueIdBuilder(jToken,idBuilder);

                foreach (var eachKVP in (JObject)jToken)
                {
                    //add each property to that hashtable
                    subObjectH.Add(eachKVP.Key.ToLower(), UnpackToken(eachKVP.Value, subObjectBuilder));
                }

                //return the hashtable so it can be added in its turn, with the unpacked object
                return subObjectH;
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
