

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
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
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

    

    public ContentImportLog PopulateCompendium(ICompendium compendiumToPopulate)
    {
        Dictionary<string,EntityTypeDataLoader> dataLoaders=new Dictionary<string,EntityTypeDataLoader>();
        List<Type> importableEntityTypes=new List<Type>();
        var assembly = Assembly.GetExecutingAssembly();

        var coreContentFileLoader=new ContentFileLoader(CORE_CONTENT_DIR);

        var locContentFileLoader = new ContentFileLoader(LOC_CONTENT_DIR.Replace("[culture]", LanguageTable.targetCulture));

        coreContentFileLoader.LoadContentFiles();
        locContentFileLoader.LoadContentFiles();

        

        foreach (Type type in assembly.GetTypes())
        {
            FucineImportable importableAttribute =
                (FucineImportable) type.GetCustomAttribute(typeof(FucineImportable), false);
            if (importableAttribute != null)
            {
                EntityTypeDataLoader loader=new EntityTypeDataLoader(type,importableAttribute.TaggedAs,LanguageTable.targetCulture,_log);
                dataLoaders.Add(importableAttribute.TaggedAs.ToLower(),loader);
                importableEntityTypes.Add(type);
            }
        }

        //We've identified the entity types: now set the compendium up for these
        compendiumToPopulate.InitialiseForEntityTypes(importableEntityTypes);
        

        foreach (EntityTypeDataLoader dataLoaderForEntityType in dataLoaders.Values)
        {

            var coreContentFiles= coreContentFileLoader.GetLoadedContentFilesContainingEntityTag(dataLoaderForEntityType.EntityTag);
            var locContentFiles =
                locContentFileLoader.GetLoadedContentFilesContainingEntityTag(dataLoaderForEntityType.EntityTag);


            dataLoaderForEntityType.SupplyContentFiles(coreContentFiles, locContentFiles);

            
            dataLoaderForEntityType.LoadCoreData();
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


