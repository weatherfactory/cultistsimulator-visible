﻿

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
    private static readonly string LOC_CONTENT_DIR = Application.streamingAssetsPath + "/content/core_[culture]/";
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


    private List<string> GetContentFilesRecursive(string path)
    {
        List<string> contentFiles = new List<string>();
        //find all the content files
        if(Directory.Exists(path))
        {
            contentFiles.AddRange(Directory.GetFiles(path).ToList().FindAll(f => f.EndsWith(".json")));
            foreach (var subdirectory in Directory.GetDirectories(path))
                contentFiles.AddRange(GetContentFilesRecursive(subdirectory));
        }
        return contentFiles;
    }
    private string GetBaseFolderForLocalisedData(string culture)
    {
        return LOC_CONTENT_DIR.Replace("[culture]", culture);
    }


    public ContentImportLog PopulateCompendium(ICompendium compendiumToPopulate)
    {
        List<EntityTypeDataLoader> dataLoaders=new List<EntityTypeDataLoader>();
        List<Type> importableEntityTypes=new List<Type>();
        var assembly = Assembly.GetExecutingAssembly();


        //find all the content files
        var coreContentFiles = GetContentFilesRecursive(CORE_CONTENT_DIR);

        if (coreContentFiles.Any())
            coreContentFiles.Sort();

        //find all the loc files
        var locContentPath = GetBaseFolderForLocalisedData(LanguageTable.targetCulture);
        var locContentFiles = GetContentFilesRecursive(locContentPath);
            if(locContentFiles.Any())
                locContentFiles.Sort();
        //find all the mod files





        foreach (Type type in assembly.GetTypes())
        {
            FucineImportable importableAttribute =
                (FucineImportable) type.GetCustomAttribute(typeof(FucineImportable), false);
            if (importableAttribute != null)
            {
                dataLoaders.Add(new EntityTypeDataLoader(type,importableAttribute.TaggedAs, LanguageTable.targetCulture, _log));
                importableEntityTypes.Add(type);
            }
        }

        //We've identified the entity types: now set the compendium up for these
        compendiumToPopulate.InitialiseForEntityTypes(importableEntityTypes);




        foreach (EntityTypeDataLoader dataLoaderForEntityType in dataLoaders)
        {
            
                dataLoaderForEntityType.LoadCoreData(coreContentFiles,locContentFiles);
             //   dataLoaderForEntityType.LoadModData();

            foreach (EntityData entityData in dataLoaderForEntityType.Entities)
                {
                    IEntityWithId newEntity = FactoryInstantiator.CreateEntity(dataLoaderForEntityType.EntityType, entityData, _log);
                    if(!compendiumToPopulate.TryAddEntity(newEntity))
                        _log.LogWarning($"Can't add entity {newEntity.Id} of type {newEntity.GetType()}");
                }


                if (_log.GetMessages().Any(m => m.MessageLevel > 1))
                    //found a serious problem: bug out and report.
                    return _log;
        }


        compendiumToPopulate.OnPostImport(_log);

        return _log;


    }



}


