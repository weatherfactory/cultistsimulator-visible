using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noon;
using OrbCreationExtensions;
using UnityEngine;

namespace Assets.Core.Fucine
{
    public class DataImporterForEntity
    {
        private static readonly string CORE_CONTENT_DIR = Application.streamingAssetsPath + "/content/core/";


        public readonly string EntityFolder;
        private readonly ContentImportLog _log;
        public ArrayList OriginalData { get; set; }
        public ArrayList LocalisedData { get; set; }
        public string BaseCulture { get; } = "en";
        public string CurrentCulture { get; set; }


        public string GetBaseFolderForLocalisedData()
        {
            return "core_" + CurrentCulture;
        }


        public DataImporterForEntity(string entityFolder, string currentCulture, ContentImportLog log)
        {
            EntityFolder = entityFolder;
            _log = log;
            this.CurrentCulture = currentCulture;
            OriginalData = new ArrayList();
            LocalisedData = new ArrayList();
        }


        public void LoadEntityData()
        {
            var contentFolder = CORE_CONTENT_DIR + EntityFolder;
            var coreContentFiles = Directory.GetFiles(contentFolder).ToList().FindAll(f => f.EndsWith(".json"));
            if (coreContentFiles.Any())
                coreContentFiles.Sort();



            GetContentItemsWithLocalisation(EntityFolder, coreContentFiles, _log);

            var contentImportForMods = new ContentImportForMods();
            contentImportForMods.ProcessContentItemsWithMods(this.OriginalData, EntityFolder);
        }



        public void GetContentItemsWithLocalisation(string contentOfType, List<string> coreContentFiles, ContentImportLog log)
        {


            //allcontentfiles contains both core and override json
            List<string> allContentFiles = new List<string>();
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
                    OriginalData.AddRange(SimpleJsonImporter.Import(json, true).GetArrayList(contentOfType));
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
                        LocalisedData.AddRange(SimpleJsonImporter.Import(json, true)
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
                    thisIsATemporaryHomeForThisMethod.CopyFields(OriginalData,
                        LocalisedData, fieldsToTranslate,
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
                                    OriginalData);
                                //FileUtil.ReplaceFile(outputFile,locFile);			// Hard replace
                            }
                            else
                            {
                                thisIsATemporaryHomeForThisMethod.Export(locFile, contentOfType,
                                    OriginalData);
                            }

                            NoonUtility.Log("Exported [" + locFile + "]");
                        }
                    }
                }
            }
        }
























    }
}
