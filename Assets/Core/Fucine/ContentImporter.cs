
//#define LOC_AUTO_REPAIR		// Useful for importing fresh loc data into game. Merges current data with localised strings and outputs partially localised files with new data annotated - CP
								// NB. Running autorepair repeatedly will flush the "NEW" comments out because it modifies the source data in-place,
								// so on the second run the added hashtables are not considered new.
								// Enable this #define...run ONCE on target language, then turn it off again to test the autorepaired data.

using UnityEngine;
using System;
using Noon;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Newtonsoft.Json;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using OrbCreationExtensions;
using Unity.Profiling;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif




public class ContentImporter
{
    
    private static readonly string CORE_CONTENT_DIR = Application.streamingAssetsPath + "/content/core/";
    private const string CONST_LEGACIES = "legacies"; //careful: this is specified in the Legacy FucineImport attribute too
    private static readonly Regex DlcLegacyRegex = new Regex(@"DLC_(\w+)_\w+_legacy\.json");
    readonly ContentImportLog _log=new ContentImportLog();


    public static IEnumerable<string> GetInstalledDlc()
    {
        return from path in Directory.GetFiles(Path.Combine(CORE_CONTENT_DIR, CONST_LEGACIES)) 
            select Path.GetFileName(path) into fileName 
            select DlcLegacyRegex.Match(fileName) into match 
            where match.Success 
            select match.Groups[1].Value;
    }



    private ArrayList GetEntityDataFromFiles(string entityFolder)
    {
        var contentFolder = CORE_CONTENT_DIR + entityFolder;
        var coreContentFiles = Directory.GetFiles(contentFolder).ToList().FindAll(f => f.EndsWith(".json"));
        if(coreContentFiles.Any())
          coreContentFiles.Sort();

        DataImporterForEntity dataImporterForEntity = new DataImporterForEntity(entityFolder, LanguageTable.targetCulture);


        dataImporterForEntity.GetContentItemsWithLocalisation(entityFolder, coreContentFiles,_log);

        var contentImportForMods=new ContentImportForMods();
        return contentImportForMods.ProcessContentItemsWithMods(dataImporterForEntity.OriginalData, entityFolder);
    }

    


    public ContentImportLog PopulateCompendium(ICompendium compendiumToPopulate)
    {
        compendiumToPopulate.Reset();
        var assembly = Assembly.GetExecutingAssembly();

       foreach (Type T in assembly.GetTypes())
        {
            FucineImportable importableAttribute = (FucineImportable) T.GetCustomAttribute(typeof(FucineImportable), false);
               
            if(importableAttribute!=null)
            {
                ArrayList al = GetEntityDataFromFiles(importableAttribute.TaggedAs);

                foreach (Hashtable h in al)
                {
                    IEntityWithId newEntity = FactoryInstantiator.CreateEntity(T, h, _log);
                    compendiumToPopulate.AddEntity(newEntity.Id,T, newEntity);
                }
            }

            if (_log.GetMessages().Any(m => m.MessageLevel > 1))
                //found a serious problem: bug out and report.
                return _log;
        }


        compendiumToPopulate.OnPostImport(_log);

        return _log;


    }


}


