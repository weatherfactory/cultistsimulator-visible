using System;
using System.Collections;
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


        public readonly string EntityFolder;
        private readonly ContentImportLog _log;
        public ArrayList CoreData { get; set; }
        public ArrayList LocalisedValuesData { get; set; }
        public string BaseCulture { get; } = "en";
        public string CurrentCulture { get; set; }


        public string GetBaseFolderForLocalisedData()
        {
            return "core_" + CurrentCulture;
        }


        public DataProviderForEntityType(string entityFolder, string currentCulture, ContentImportLog log)
        {
            EntityFolder = entityFolder;
            _log = log;
            this.CurrentCulture = currentCulture;
            CoreData = new ArrayList();
            LocalisedValuesData = new ArrayList();
        }


        public void LoadEntityData()
        {
            var contentFolder = CORE_CONTENT_DIR + EntityFolder;
            var coreContentFiles = Directory.GetFiles(contentFolder).ToList().FindAll(f => f.EndsWith(".json"));
            if (coreContentFiles.Any())
                coreContentFiles.Sort();
            GetCoreAndLocDataForContentType(EntityFolder, coreContentFiles, _log);

            var contentImportForMods = new ContentImportForMods();
            contentImportForMods.ProcessContentItemsWithMods(this.CoreData, EntityFolder);
        }



        public void GetCoreAndLocDataForContentType(string contentOfType, System.Collections.Generic.List<string> coreContentFiles, ContentImportLog log)
        {


            //allcontentfiles contains both core and override json
            System.Collections.Generic.List<string> allContentFiles = new System.Collections.Generic.List<string>();
            allContentFiles.AddRange(coreContentFiles);
            if (!allContentFiles.Any()) log.LogProblem("Can't find any " + contentOfType + " to import as content");

            //into alpha order rather than core followed by override
            allContentFiles.Sort();


            foreach (var contentFile in allContentFiles)
            {
                //json string for each content file - in English initially
                string json = File.ReadAllText(contentFile);


                try
                {

                  JToken topLevelObject=JObject.Parse(json);

                  JArray topLevelArrayList = (JArray) topLevelObject[contentOfType];


                    foreach (var eachObject in topLevelArrayList)
                  {

                      Hashtable h = new Hashtable();

                      foreach (var eachKVP in (JObject)eachObject)
                          AddTokenToHashtable(eachKVP.Key, h, eachKVP.Value);

                      EntityData entityData = new EntityData(h);

                      CoreData.Add(entityData);
                  } 
                    
                    
                }
                catch (Exception e)
                {
                    log.LogProblem("This file broke: " + contentFile + " with error " + e.Message);
                }

                if (BaseCulture != CurrentCulture)
                    TryToLocalise(contentOfType, log, contentFile);
            }

        }

        private void TryToLocalise(string contentOfType, ContentImportLog log, string contentFile)
        {
            string json;
            string
                locFolder = "core_" + LanguageTable.targetCulture; //ahem. Let's move this to a LocalisedText object or similar
            string locFile = contentFile;
            locFile = locFile.Replace("core", locFolder); //ahem, further. - AK
            if (File.Exists(locFile)) // If no file exists, no localisation happens
            {
                //get the json string from the localised file. This shouild probably use the same code as above
                json = File.ReadAllText(locFile);
                if (json.Length > 0)
                {
                    try
                    {
                        //yup, still the same
                        LocalisedValuesData.AddRange(SimpleJsonImporter.Import(json, true)
                            .GetArrayList(contentOfType));
                    }
                    catch (Exception e)
                    {
                        log.LogProblem("This file broke: " + contentFile + " with error " + e.Message);
                    }


                    bool repair = false;
                    bool changed = false;
#if UNITY_EDITOR && LOC_AUTO_REPAIR //all this should be moved to a button on debug that appears only in the editor
						repair = true;
#endif
                    // We now have two sets of data which SHOULD match pair for pair - english and translated.
                    // Traverse the dataset copying the following fields into the core data. Add new fields here if they need translating.
                    // If the field is a list it will have ALL contents inside localised
                    string[] fieldsToTranslate = { "label", "description", "startdescription", "drawmessages" };

                    //
                    // COPY LOCALISATION DATA INTO originalArrayList
                    //
                    var thisIsATemporaryHomeForThisMethod = new ContentImportForMods();
                    thisIsATemporaryHomeForThisMethod.CopyFields(CoreData,
                        LocalisedValuesData, fieldsToTranslate,
                        false, repair, ref changed);

                    if (repair)
                    {
                        NoonUtility.Log("Localising [" + locFile + "]"); //AK: I think this should be here?
                                                                         //(a) we don't actually autofix the file unless one is missing, and
                                                                         //(b) the log is currently showing messages about the /more files, which shouldn't be localised to /core anyway.
                        if (changed)
                        {
                            bool testOutput = false;
                            if (testOutput)
                            {
                                /*
                            string backupFile = locFile.Replace( ".json", "_backup.json" );
                            if (!File.Exists(backupFile))
                            {
                                FileUtil.CopyFileOrDirectory(locFile,backupFile);	// Soft backup - skip if already there
                            }
                            */
                                string outputFile = locFile.Replace(".json", "_out.json");
                                thisIsATemporaryHomeForThisMethod.Export(outputFile, contentOfType,
                                    CoreData);
                                //FileUtil.ReplaceFile(outputFile,locFile);			// Hard replace
                            }
                            else
                            {
                                thisIsATemporaryHomeForThisMethod.Export(locFile, contentOfType,
                                    CoreData);
                            }

                            NoonUtility.Log("Exported [" + locFile + "]");
                        }
                    }
                }
            }
        }




        private void AddTokenToHashtable(string id, Hashtable currentH, JToken jToken)
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
                    AddTokenToArray(nextList, eachItem);
                currentH.Add(id, nextList);

            }

            else if (jToken.Type == JTokenType.Object)
            {
                var nextH = new Hashtable();
                foreach (var eachKVP in (JObject)jToken)
                    AddTokenToHashtable(eachKVP.Key, nextH, eachKVP.Value);

                currentH.Add(id, nextH);

            }

            else
            {
                throw new ApplicationException("Unexpected jtoken type: " + jToken.Type);
            }
        }

        private void AddTokenToArray(ArrayList currentList, JToken jToken)
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
                    AddTokenToArray(nextList, eachItem);

                currentList.Add(nextList);
            }

            else if (jToken.Type == JTokenType.Object)
            {
                var nextHashtable = new Hashtable();
                foreach (var eachKVP in (JObject)jToken)
                    AddTokenToHashtable(eachKVP.Key, nextHashtable, eachKVP.Value);

                currentList.Add(nextHashtable);
            }
            else
            {
                throw new ApplicationException("Unexpected jtoken type: " + jToken.Type);
            }
        }



















    }
}
