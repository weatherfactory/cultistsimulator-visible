

using UnityEngine;
using System;
using Noon;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Newtonsoft.Json;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Newtonsoft.Json.Linq;
using OrbCreationExtensions;
using Unity.Profiling;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif




public class CompendiumLoader
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



    public ContentImportLog PopulateCompendium(ICompendium compendiumToPopulate)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var importableTypes =
            assembly.GetTypes().Where(t => t.GetCustomAttribute(typeof(FucineImportable), false) != null);

        compendiumToPopulate.InitialiseForEntityTypes(importableTypes);


       foreach (Type T in compendiumToPopulate.GetEntityTypes())
        {
            FucineImportable importableAttribute = (FucineImportable) T.GetCustomAttribute(typeof(FucineImportable), false);
               

                 DataLoaderForEntityType dataLoaderForEntityType = new DataLoaderForEntityType(importableAttribute.TaggedAs, LanguageTable.targetCulture,_log);

                dataLoaderForEntityType.LoadEntityDataFromJson();


                foreach (EntityData entityData in dataLoaderForEntityType.Entities)
                {
                    IEntityWithId newEntity = FactoryInstantiator.CreateEntity(T, entityData, _log);
                    compendiumToPopulate.AddEntity(newEntity.Id,T, newEntity);
                }


            if (_log.GetMessages().Any(m => m.MessageLevel > 1))
                //found a serious problem: bug out and report.
                return _log;
        }


        compendiumToPopulate.OnPostImport(_log);

        return _log;


    }



}


