

using UnityEngine;
using System;

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.UI;
using SecretHistories.Constants.Modding;
using SecretHistories.Services;
using OrbCreationExtensions;
using SecretHistories.Constants;
using Unity.Profiling;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class CompendiumLoader
{
    
    readonly ContentImportLog _log=new ContentImportLog();
    private readonly string _contentFolderName;

    readonly List<DataFileLoader> modContentLoaders = new List<DataFileLoader>();
    readonly List<DataFileLoader> modLocLoaders = new List<DataFileLoader>();

    public CompendiumLoader(string contentFolderName)
    {
        _contentFolderName = contentFolderName;
    }


    public ContentImportLog PopulateCompendium(Compendium compendiumToPopulate,string forCultureId)
    {
      string coreContentDir = Path.Combine(Application.streamingAssetsPath,
      _contentFolderName, NoonConstants.CORE_FOLDER_NAME);

      string locContentDir = Path.Combine(Application.streamingAssetsPath,
        _contentFolderName, NoonConstants.LOC_FOLDER_TEMPLATE);


    Dictionary<string,EntityTypeDataLoader> dataLoaders=new Dictionary<string,EntityTypeDataLoader>();
        List<Type> importableEntityTypes=new List<Type>();

        var assembly =  Assembly.GetAssembly(GetType());

        //retrieve base content for game
        var coreFileLoader=new DataFileLoader(coreContentDir);
        coreFileLoader.LoadFilesFromAssignedFolder(_log);

        //retrieve loc content for current language
        string locContentFolderForCulture = locContentDir.Replace("[culture]", forCultureId);
        var locFileLoader = new DataFileLoader(locContentFolderForCulture);
        locFileLoader.LoadFilesFromAssignedFolder(_log);
        
        if (Watchman.Exists<ModManager>())
            LoadModsToCompendium(locContentFolderForCulture);

        
        //what entities need data importing?
        foreach (Type type in assembly.GetTypes())
        {
            FucineImportable importableAttribute =
                (FucineImportable) type.GetCustomAttribute(typeof(FucineImportable), false);
            if (importableAttribute != null)
            {
                //for each importable entity:
                //create a data loader for that entity
                EntityTypeDataLoader loader=new EntityTypeDataLoader(type,importableAttribute.TaggedAs, forCultureId, _log);
                
                //add the loader and the entity type to collecitons so we can process them in a moment
                dataLoaders.Add(importableAttribute.TaggedAs.ToLower(),loader);
                importableEntityTypes.Add(type);
            }
        }

        compendiumToPopulate.InitialiseForEntityTypes(importableEntityTypes);
   
        
        foreach (EntityTypeDataLoader dl in dataLoaders.Values)
        {
            //for every entity loader:
            //get the content, the loc, and the mod files for that entity type

            var coreContentFilesForEntityForThisEntityType= coreFileLoader.GetLoadedContentFilesContainingEntityTag(dl.EntityTag);
            var locContentFilesForThisEntityType =
                locFileLoader.GetLoadedContentFilesContainingEntityTag(dl.EntityTag);

            var modContentFiles = new List<LoadedDataFile>();
            foreach(var mcfl in modContentLoaders)
                modContentFiles.AddRange(mcfl.GetLoadedContentFilesContainingEntityTag(dl.EntityTag));

            var modLocFiles = new List<LoadedDataFile>();
            foreach(var mll in modLocLoaders)
                modLocFiles.AddRange(mll.GetLoadedContentFilesContainingEntityTag(dl.EntityTag));

            
            dl.SupplyContentFiles(coreContentFilesForEntityForThisEntityType, locContentFilesForThisEntityType,modContentFiles,modLocFiles);
    
            dl.LoadEntityDataFromSuppliedFiles();

            AddImportedEntityInstancesToCompendium(compendiumToPopulate, dl);


            if (_log.GetMessages().Any(m => m.MessageLevel > 1))
               // found a serious problem: bug out and report.
                return _log;
        }


        compendiumToPopulate.OnPostImport(_log);

        if(Watchman.Exists<Concursum>())
        {
            //notify the rest of the application that content has been updated
            var concursum = Watchman.Get<Concursum>();
            concursum.ContentUpdatedEvent.Invoke(new ContentUpdatedArgs{Message = "Loaded compendium content."});
        }

        return _log;


    }

    private void AddImportedEntityInstancesToCompendium(Compendium compendiumToPopulate, EntityTypeDataLoader dl)
    {
        foreach (EntityData entityData in dl.GetLoadedEntityDataAsList())
        {
            IEntityWithId newEntity = FactoryInstantiator.CreateEntity(dl.EntityType, entityData, _log);
            if (!compendiumToPopulate.TryAddEntity(newEntity))
                _log.LogWarning($"Can't add entity {newEntity.Id} of type {newEntity.GetType()}");
        }
    }

    private void LoadModsToCompendium(string locContentFolderForCulture)
    {
        var modManager = Watchman.Get<ModManager>();

        modManager.CatalogueMods();
        foreach (var mod in modManager.GetEnabledModsInLoadOrder())
        {
            var modContentLoader = new DataFileLoader(mod.ContentFolder);
            modContentLoader.LoadFilesFromAssignedFolder(_log);
            modContentLoaders.Add(modContentLoader);
            string pathForCurrentlyRelevantLoc =
                GetModCurrentlyRelevantLocPath(mod, locContentFolderForCulture);
            var modLocLoader = new DataFileLoader(pathForCurrentlyRelevantLoc);
            modLocLoader.LoadFilesFromAssignedFolder(_log);
            modLocLoaders.Add(modLocLoader);
        }
    }

    private string GetModCurrentlyRelevantLocPath(Mod mod, string locContentFolderForCulture)
    {
        string modLocPath = Path.Combine(mod.LocFolder,
            locContentFolderForCulture); //there is a locContentFolderForCulture, eg "/loc_fr", in core and in each mod folder. The mod needs to load the loc for the current culture, not the root loc folder!

        return modLocPath;
    }
}


