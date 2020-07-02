
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
#if MODS
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
#endif
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
    private static readonly string MORE_CONTENT_DIR = Application.streamingAssetsPath + "/content/more/";
    private const string CONST_LEGACIES = "legacies"; //careful: this is specified in the Legacy FucineImport attribute too


    public Dictionary<string, IVerb> Verbs;
    public Dictionary<string, Element> Elements;
    public Dictionary<string, Legacy> Legacies;
    public Dictionary<string, Ending> Endings;
    public List<Recipe> Recipes;
    private Dictionary<string, IDeckSpec> DeckSpecs;
    
    private static readonly Regex DlcLegacyRegex = new Regex(@"DLC_(\w+)_\w+_legacy\.json");

    ContentImportLog _log=new ContentImportLog();


    public ContentImporter()
    {
       
        Verbs = new Dictionary<string, IVerb>();
        Elements = new Dictionary<string, Element>();
        Recipes = new List<Recipe>();
        DeckSpecs = new Dictionary<string, IDeckSpec>();
        Legacies = new Dictionary<string, Legacy>();
		Endings = new Dictionary<string, Ending>();
    }

    public static IEnumerable<string> GetInstalledDlc()
    {
        return from path in Directory.GetFiles(Path.Combine(CORE_CONTENT_DIR, CONST_LEGACIES)) 
            select Path.GetFileName(path) into fileName 
            select DlcLegacyRegex.Match(fileName) into match 
            where match.Success 
            select match.Groups[1].Value;
    }



    private ArrayList GetContentItems(string contentOfType)
    {
        var contentFolder = CORE_CONTENT_DIR + contentOfType;
        var contentOverrideFolder = MORE_CONTENT_DIR + contentOfType;
        var coreContentFiles = Directory.GetFiles(contentFolder).ToList().FindAll(f => f.EndsWith(".json"));
        if(coreContentFiles.Any())
          coreContentFiles.Sort();
        var overridecontentFiles = Directory.GetFiles(contentOverrideFolder).ToList().FindAll(f => f.EndsWith(".json"));
        if(overridecontentFiles.Any())
             overridecontentFiles.Sort();

        
        var contentItemArrayList = ContentImportForLoc.GetContentItemsWithLocalisation(contentOfType, coreContentFiles, overridecontentFiles,_log);
#if MODS
        var contentImportForMods=new ContentImportForMods();
        return contentImportForMods.ProcessContentItemsWithMods(contentItemArrayList, contentOfType);
#else
        return contentItemArrayList;
#endif
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
                ArrayList al = GetContentItems(importableAttribute.TaggedAs);

                foreach (Hashtable h in al)
                {
                    IEntityWithId newEntity = FactoryInstantiator.CreateEntity(T, h, _log);
                    compendiumToPopulate.AddEntity(newEntity.Id,T, newEntity);
                }
            }

            if (_log.GetMessages().Any(m => m.MessageLevel > 1))
                //serious problem: bug out and report.
                return _log;
        }


        compendiumToPopulate.OnPostImport(_log);

        return _log;


    }


}


