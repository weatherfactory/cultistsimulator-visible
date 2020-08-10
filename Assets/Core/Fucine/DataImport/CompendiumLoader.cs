

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
using Assets.TabletopUi.Scripts.Services;
using OrbCreationExtensions;
using Unity.Profiling;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class CompendiumLoader
{

    private static readonly string CORE_CONTENT_DIR = Path.Combine(Application.streamingAssetsPath,
        NoonConstants.CONTENT_FOLDER_NAME, NoonConstants.CORE_FOLDER_NAME);

    private static readonly string LOC_CONTENT_DIR = Path.Combine(Application.streamingAssetsPath,
        NoonConstants.CONTENT_FOLDER_NAME, NoonConstants.LOC_FOLDER_TEMPLATE);

    readonly ContentImportLog _log=new ContentImportLog();


    public ContentImportLog PopulateCompendium(ICompendium compendiumToPopulate)
    {

        Dictionary<string,EntityTypeDataLoader> dataLoaders=new Dictionary<string,EntityTypeDataLoader>();
        List<Type> importableEntityTypes=new List<Type>();
        var assembly = Assembly.GetExecutingAssembly();

        //retrieve base content for game
        var coreFileLoader=new ContentFileLoader(CORE_CONTENT_DIR);
        coreFileLoader.LoadContentFiles(_log);

        //retrieve loc content for current language
        var locFileLoader = new ContentFileLoader(LOC_CONTENT_DIR.Replace("[culture]", LanguageTable.targetCulture));
        locFileLoader.LoadContentFiles(_log);
        

        //retrieve contents of all mod files
        List<ContentFileLoader> modFileLoaders=new List<ContentFileLoader>();
        var modManager = Registry.Retrieve<ModManager>();
        modManager.CatalogueMods();
        foreach (var mod in modManager.GetEnabledMods())
        {
            var modFileLoader=new ContentFileLoader(mod.ContentFolder);
            modFileLoader.LoadContentFiles(_log);
            modFileLoaders.Add(modFileLoader);
        }
        

        
        //what entities need data importing?
        foreach (Type type in assembly.GetTypes())
        {
            FucineImportable importableAttribute =
                (FucineImportable) type.GetCustomAttribute(typeof(FucineImportable), false);
            if (importableAttribute != null)
            {
                //for each importable entity:
                //create a data loader for that entity
                EntityTypeDataLoader loader=new EntityTypeDataLoader(type,importableAttribute.TaggedAs,LanguageTable.targetCulture,_log);
                
                //add the loader and the entity type to collecitons so we can process them in a moment
                dataLoaders.Add(importableAttribute.TaggedAs.ToLower(),loader);
                importableEntityTypes.Add(type);
            }
        }

        //We've identified the entity types: now set the compendium up to store these types
        compendiumToPopulate.InitialiseForEntityTypes(importableEntityTypes);
        

        
        foreach (EntityTypeDataLoader dl in dataLoaders.Values)
        {
            //for every entity loader:
            //get the content, the loc, and the mod files for that entity type

            var coreContentFilesForEntityForThisEntityType= coreFileLoader.GetLoadedContentFilesContainingEntityTag(dl.EntityTag);
            var locContentFilesForThisEntityType =
                locFileLoader.GetLoadedContentFilesContainingEntityTag(dl.EntityTag);

            var modContentFiles = new List<LoadedContentFile>();
            foreach(var mcfl in modFileLoaders)
                modContentFiles.AddRange(mcfl.GetLoadedContentFilesContainingEntityTag(dl.EntityTag));

            

            dl.SupplyContentFiles(coreContentFilesForEntityForThisEntityType, locContentFilesForThisEntityType,modContentFiles);

            
            dl.LoadEntityDataFromSuppliedFiles();
             //   dataLoaderForEntityType.LoadModData();

            foreach (EntityData entityData in dl.GetLoadedEntityDataAsList())
            {
                IEntityWithId newEntity = FactoryInstantiator.CreateEntity(dl.EntityType, entityData, _log);
                if(!compendiumToPopulate.TryAddEntity(newEntity))
                    _log.LogWarning($"Can't add entity {newEntity.Id} of type {newEntity.GetType()}");
            }


            if (_log.GetMessages().Any(m => m.MessageLevel > 1))
                //found a serious problem: bug out and report.
                return _log;
        }


        compendiumToPopulate.OnPostImport(_log);

        
        //notify the rest of the application that content has been updated
        var concursum = Registry.Retrieve<Concursum>();
        concursum.ContentUpdated(new ContentUpdatedArgs{Message = "Loaded compendium content."});


        return _log;


    }



}


