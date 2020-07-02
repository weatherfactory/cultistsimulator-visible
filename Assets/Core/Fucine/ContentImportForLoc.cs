using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    class ContentImportForLoc
    {



        public static ArrayList GetContentItemsWithLocalisation(string contentOfType, List<string> coreContentFiles, List<string> overridecontentFiles, ContentImportLog log)
        {
            ArrayList contentItemArrayList = new ArrayList();

            List<string> allContentFiles = new List<string>();
            allContentFiles.AddRange(coreContentFiles);
            allContentFiles.AddRange(overridecontentFiles);
            if (!allContentFiles.Any()) log.LogProblem("Can't find any " + contentOfType + " to import as content");


            ArrayList originalArrayList = new ArrayList();
            ArrayList localisedArrayList = new ArrayList();

            //into alpha order
            allContentFiles.Sort();

            foreach (var contentFile in allContentFiles)
            {
                //json for each content file
                string json = File.ReadAllText(contentFile);
                try
                {
                    //populate relevant array list
                    originalArrayList = SimpleJsonImporter.Import(json, true).GetArrayList(contentOfType);
                }
                catch (Exception e)
                {
                    log.LogProblem("This file broke: " + contentFile + " with error " + e.Message);
                    originalArrayList = new ArrayList();
                }

                // Now look for localised language equivalent of the same file and parse that
                string locFolder = "core_" + LanguageTable.targetCulture; //ahem. - AK
                string locFile = contentFile;
                locFile = locFile.Replace("core", locFolder); //ahem, further. - AK
                if (File.Exists(locFile)) // If no file exists, no localisation happens
                {
                    json = File.ReadAllText(locFile);
                    if (json.Length > 0)
                    {
                        try
                        {
                            localisedArrayList = SimpleJsonImporter.Import(json, true).GetArrayList(contentOfType);
                        }
                        catch (Exception e)
                        {
                            log.LogProblem("This file broke: " + contentFile + " with error " + e.Message);
                            localisedArrayList = new ArrayList();
                        }


                        bool repair = false;
                        bool changed = false;
#if UNITY_EDITOR && LOC_AUTO_REPAIR
					//if (locFile.EndsWith("events.json"))
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
                        thisIsATemporaryHomeForThisMethod.CopyFields(originalArrayList, localisedArrayList, fieldsToTranslate,
                            false, repair, ref changed);

#if UNITY_EDITOR && LOC_AUTO_REPAIR
NoonUtility.Log("Localising ["+ locFile +"]");  //AK: I think this should be here?
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
							string outputFile = locFile.Replace( ".json", "_out.json" );
							Export( outputFile, contentOfType, originalArrayList );
							//FileUtil.ReplaceFile(outputFile,locFile);			// Hard replace
						}
						else
						{
							Export( locFile, contentOfType, originalArrayList );
						}
						NoonUtility.Log("Exported ["+ locFile +"]");
					}
#endif
                    }
                }

                contentItemArrayList.AddRange(originalArrayList);
            }

            return contentItemArrayList;
        }
    }
}
