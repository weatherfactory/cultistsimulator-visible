using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Services;

using UnityEngine;

namespace SecretHistories.Constants.Modding
{
    /// <summary>
    /// Handles location, enablement and manifest verification of mods
    /// Actual entity loading is handled by the entity data loading classes
    /// </summary>
    [Immanence(typeof(ModManager))]
    public class ModManager
    {
        private const string MOD_SYNOPSIS_FILENAME = "synopsis.json";
        private const string MOD_DLL_FOLDER = "dll";
        private const string MOD_DLL_SUFFIX = ".dll";
        private const string MOD_DLL_NAME_DEPRECATED= "main";


        private bool AssemblyLoadingForModsAttempted = false;


        private static readonly string LocalModsPath = Path.Combine(Application.persistentDataPath, "mods");

        
        private static readonly string ModEnabledListPath = Path.Combine(Application.persistentDataPath, "mods.txt");

        private readonly Dictionary<string, Mod> _cataloguedMods;
        private List<string> _enabledModsLoadOrder;

        public void LoadModDLLs()
        {

            if (AssemblyLoadingForModsAttempted)
            {
                NoonUtility.LogWarning("Tried to load assemblies for mods twice. We don't want that.");
                return;
            }

            AssemblyLoadingForModsAttempted = true;

            var enabledModsInLoadOrder = Watchman.Get<ModManager>().GetEnabledModsInLoadOrder();

            foreach (var mod in enabledModsInLoadOrder)
                TryLoadDllsForMod(mod);

            foreach(var mod in enabledModsInLoadOrder.Where(m=>m.ValidAssemblyIsLoaded()))
                mod.TryInitialiseAssembly();
            
        }

        private void TryLoadDllsForMod(Mod mod)
        {
           
            string preferredDllPath = Path.Combine(mod.ModRootFolder, MOD_DLL_FOLDER,
                $"{mod.GetNameAlphanumericsOnly()}{MOD_DLL_SUFFIX}");
            string deprecatedDllPath = Path.Combine(mod.ModRootFolder, MOD_DLL_FOLDER,
                $"{MOD_DLL_NAME_DEPRECATED}{MOD_DLL_SUFFIX}");

            
            string dllFoundPath;
            if (File.Exists(preferredDllPath)) 
                dllFoundPath = preferredDllPath;
            else if (File.Exists(deprecatedDllPath))
                dllFoundPath = deprecatedDllPath;
            else
             return;

            mod.TryLoadAssembly(dllFoundPath);
        }

        public ModManager()
        {
            _cataloguedMods = new Dictionary<string, Mod>();
        }

        public IEnumerable<Mod> GetCataloguedMods()
        {
            var cataloguedMods = _cataloguedMods.Values;
            return cataloguedMods;
        }

        public IEnumerable<Mod> GetEnabledModsInLoadOrder()
        {
            var orderedIds = GetEnabledModsLoadOrderList();
            var orderedMods=new List<Mod>();
            foreach (var oi in orderedIds)
            {
                if (_cataloguedMods.ContainsKey(oi) && orderedMods.All(om => om.Id != oi)) // second clause is in case something broke and a duplicate snuck in
                    orderedMods.Add(_cataloguedMods[oi]);
                else
                    NoonUtility.LogWarning(
                        $"Problem getting enabled mod lists: {oi} was found in the enabled loading order list, but not in the catalogue");

            }

            return orderedMods;
        }

        public IEnumerable<Mod> GetDisabledMods()
        {
            var disabledMods = new List<Mod>(); //we could just filter by enabled=true, but I'd rather treat the mods order list as the authoritative source of info and enabled as local info when displaying mod entries
            var orderedIds = GetEnabledModsLoadOrderList();
            foreach (var m in _cataloguedMods)
            {
                if(!orderedIds.Contains(m.Key))
                    disabledMods.Add(m.Value);
            }
            
            return disabledMods;

        }

        public List<string> GetEnabledModsLoadOrderList()
        {
            if (_enabledModsLoadOrder == null)
            {
                NoonUtility.LogWarning("Tried to get enabled mods load order list, but it hasnn't been populated so is still null. Returning empty list of strings. Pax vobiscum");
                return new List<string>();
            }

            return new List<string>(_enabledModsLoadOrder);
        }



        public void CatalogueMods()
        {

            var storefrontServicesProvider = Watchman.Get<StorefrontServicesProvider>();

             _cataloguedMods.Clear();

            // Check if the mods folder exists
            if (!Directory.Exists(LocalModsPath))
            {
                Directory.CreateDirectory(LocalModsPath);
                NoonUtility.Log($"Mods folder not found, creating it at {LocalModsPath}", messageLevel: 1);
            }
            var localModDirectories = Directory.GetDirectories(LocalModsPath).ToList();
            var localMods = CatalogueModsInFolders(localModDirectories,ModInstallType.Local);


            var storefrontModDirectories = storefrontServicesProvider.GetSubscribedItems();
            var storefrontMods = CatalogueModsInFolders(storefrontModDirectories.Select(smd=>smd.ModRootFolder),ModInstallType.SteamWorkshop);



            foreach (var lm in localMods)
                _cataloguedMods.Add(lm.Key,lm.Value);


            foreach (var sm in storefrontMods)
            {
                if(_cataloguedMods.ContainsKey(sm.Key))
                    NoonUtility.Log($"Duplicate mod id {sm.Key} - not cataloguing duplicate instance of mod",1,VerbosityLevel.Significants);
                else
                    _cataloguedMods.Add(sm.Key,sm.Value);
            }

            // Check the dependencies to see if there are any missing or invalid ones
            foreach (var mod in _cataloguedMods)
            {
                foreach (var dependency in mod.Value.Dependencies)
                {
                    if (!_cataloguedMods.ContainsKey(dependency.ModId))
                    {
                        NoonUtility.Log(
                            "Dependency '" + dependency.ModId + "' for '" + mod.Key + "' not found ", 
                            messageLevel: 1, VerbosityLevel.Significants);
                    }
                    else
                    {
                        var availableVersion = _cataloguedMods[dependency.ModId].Version;
                        bool isVersionValid;
                        switch (dependency.VersionOperator)
                        {
                            case DependencyOperator.LessThan:
                                isVersionValid = availableVersion < dependency.Version;
                                break;
                            case DependencyOperator.LessThanOrEqual:
                                isVersionValid = availableVersion <= dependency.Version;
                                break;
                            case DependencyOperator.GreaterThan:
                                isVersionValid = availableVersion > dependency.Version;
                                break;
                            case DependencyOperator.GreaterThanOrEqual:
                                isVersionValid = availableVersion >= dependency.Version;
                                break;
                            case DependencyOperator.Equal:
                                isVersionValid = availableVersion == dependency.Version;
                                break;
                            default:
                                isVersionValid = true;
                                break;
                        }

                        if (!isVersionValid)
                        {
                            NoonUtility.Log(
                                "Dependency '" + dependency.ModId + "' for '" + mod.Key + "' has incompatible version",
                                messageLevel: 2,VerbosityLevel.Significants);
                        }
                    }
                }
            }

            //populate enabled mods load order list

            _enabledModsLoadOrder = GetEnabledModsLoadOrderFromFile().ToList();
            //remove any enabled mods that are no longer catalogued)
            foreach (var modId in new List<string>(_enabledModsLoadOrder))
            {
                if (!_cataloguedMods.ContainsKey(modId))
                    _enabledModsLoadOrder.Remove(modId);
            }
            
            // Set the enabled flag in all catalogued mods in the enabled list
            foreach (var modId in _enabledModsLoadOrder)
            {
                if (_cataloguedMods.ContainsKey(modId))
                    _cataloguedMods[modId].Enabled = true;
            }
        }

    private Dictionary<string, Mod> CatalogueModsInFolders(IEnumerable<string> inFolders,ModInstallType modInstallTypeForLocation)
    {
        var theseMods = new Dictionary<string, Mod>();

        foreach (var modFolder in inFolders)
        {
            var cataloguedMod=GetCataloguedMod(modInstallTypeForLocation, modFolder, theseMods);

            if(cataloguedMod.IsValid)
            {
                theseMods.Add(cataloguedMod.Id, cataloguedMod);
                NoonUtility.Log(cataloguedMod.CataloguingLog,0,VerbosityLevel.SystemChatter);
            }
            else
                NoonUtility.Log(cataloguedMod.CataloguingLog, 2, VerbosityLevel.Significants);
        }

            return theseMods;
    }

    private Mod GetCataloguedMod(ModInstallType modInstallTypeForLocation, string modFolder, Dictionary<string, Mod> theseMods)
    {
        var modId = Path.GetFileName(modFolder);

        var mod = new Mod(modId, modFolder);

        mod.CataloguingLog = $"Mod id {Path.GetFileName(modFolder)}: ";

            

        // Find the mod's manifest and load its data
        var synopsisPath = Path.Combine(modFolder, MOD_SYNOPSIS_FILENAME);
        if (!File.Exists(synopsisPath))
        {
            mod.CataloguingLog += "Mod synopsis not found; skipping mod";
          mod.MarkAsInvalid();
            return mod;
        }

        //TODO: refactor to JSON.NET
        var manifestData = SimpleJsonImporter.Import(File.ReadAllText(synopsisPath));
        if (manifestData == null)
        {
            mod.CataloguingLog += "Invalid mod manifest JSON; skipping mod";
            mod.MarkAsInvalid();
            return mod;
        }

   

        Sprite previewSprite = LoadSprite(mod.PreviewImageFilePath);
        if (previewSprite == null)
            mod.CataloguingLog+="has no preview image; can't upload it to Workshop;";
        else
            mod.PreviewImage = previewSprite;


        var synopsisErrors = mod.PopulateFromSynopsis(manifestData);
        if (synopsisErrors.Count > 0)
        {
            foreach (var error in synopsisErrors)
            {
                mod.CataloguingLog += $"error in synopsis: {error}; ";
            }

            mod.MarkAsInvalid();
            return mod;
        }
        
        var candidateContentFolder = Path.Combine(modFolder, Watchman.Get<Config>().GetConfigValue(NoonConstants.CONTENT_FOLDER_NAME_KEY));

        if (!Directory.Exists(candidateContentFolder))
        {
            mod.CataloguingLog += " has no content directory; ";
        }
        else
            mod.ContentFolder = candidateContentFolder;

        var candidateLocFolder = Path.Combine(modFolder, NoonConstants.LOC_FOLDER_NAME);

        if (!Directory.Exists(candidateContentFolder))
        {

            mod.CataloguingLog += " has no loc directory; ";
        }
        else
        {
            mod.CataloguingLog += " has a loc directory; ";
            mod.LocFolder = candidateLocFolder;
        }

        if (TryLoadAllImages(mod, modFolder))
            mod.CataloguingLog += " has a valid images directory; ";
        else
            mod.CataloguingLog += " has no valid images directory; ";


        mod.ModInstallType = modInstallTypeForLocation;
            

        return mod;
    }

    public Mod SetModEnableStateAndReloadContent(string modId, bool enable)
    {
 
            if (!_cataloguedMods.ContainsKey(modId))
            {
                NoonUtility.LogWarning($"Trying to disable {modId}, but it isn't in the list of catalogued mods");
                return null;
            }
            if (enable)
                _enabledModsLoadOrder.Add(modId);
            else
                _enabledModsLoadOrder.Remove(modId);

            PersistEnabledModsLoadOrderToFile();
            
            var compendiumLoader = new CompendiumLoader(Watchman.Get<Config>().GetConfigValue(NoonConstants.CONTENT_FOLDER_NAME_KEY));
            var existingCompendium = Watchman.Get<Compendium>();
            compendiumLoader.PopulateCompendium(existingCompendium, Watchman.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY));
            var modToAlter= _cataloguedMods[modId];
            modToAlter.Enabled= enable; //this should only be relevant now we're about to return it. It's already been added to the enabled mod load order list

            return modToAlter;
        }

        private IEnumerable<String> GetEnabledModsLoadOrderFromFile()
        {
            return File.Exists(ModEnabledListPath) ? File.ReadAllText(ModEnabledListPath).Split('\n') : new string[] { };
        }

        private void PersistEnabledModsLoadOrderToFile()
        {
            _enabledModsLoadOrder = _enabledModsLoadOrder.Distinct().ToList(); //clean dupes if any have snuck in
            File.WriteAllText(
                ModEnabledListPath, 
                string.Join("\n", _enabledModsLoadOrder.ToArray()));
        }

        /// <summary>
        /// returns false if there's an existing/current file id mismatch
        /// </summary>
        /// <param name="publishedMod"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public bool TryWritePublishedFileId(Mod publishedMod, string fileId)
        {


            string existingFileId = GetPublishedFileIdForMod(publishedMod);

                if (string.IsNullOrEmpty(existingFileId))
                {
                    //file doesn't exist: create it
                    File.WriteAllText(publishedMod.PublishedFileIdPath, fileId);
                    return true;
                }

                else if (existingFileId != fileId)
                {
                    NoonUtility.Log(
                        $"Upload problem: Mod {publishedMod.Id} {publishedMod.Name} was just uploaded with fileid {fileId} but file id on disk is {existingFileId}. Existing file id not overwritten.");
                    return false;
                }
                else
                //we don't need to update it
                    return true;


        }

        public string GetPublishedFileIdForMod(Mod modToCheck)
        {
            if(!File.Exists(modToCheck.PublishedFileIdPath))
                return string.Empty;

            string existingFileId = File.ReadAllText(modToCheck.PublishedFileIdPath);
                return existingFileId;
        }

        public Sprite GetSprite(string spriteResourceName)
        {
            
            foreach (var mod in _cataloguedMods.Values)
            {
                if (mod.Enabled && mod.Images.ContainsKey(spriteResourceName))
                {
                    return mod.Images[spriteResourceName];
                }
            }

            return null;
        }


        private List<string> GetFilesRecursive(string path,string extension)
        {
            List<string> FilePaths = new List<string>();
            //find all the content files
            if (Directory.Exists(path))
            {
                FilePaths.AddRange(Directory.GetFiles(path).ToList().FindAll(f => f.EndsWith(extension)));
                foreach (var subdirectory in Directory.GetDirectories(path))
                    FilePaths.AddRange(GetFilesRecursive(subdirectory,extension));
            }
            return FilePaths;
        }



        private bool TryLoadAllImages(Mod mod, string modPath)
        {

            var imagesFolderForMod = Path.Combine(modPath, "images");
            // Search all subdirectories for more image files
            
            if (Directory.Exists(imagesFolderForMod))
            {
                var imageFiles = GetFilesRecursive(imagesFolderForMod, ".png");
                if (!imageFiles.Any())
                    return false;

                foreach (var imageFile in imageFiles)
                    mod.LoadImage(imageFile);

                return true;
            }
            else
                return false;
        }


        private Sprite LoadSprite(string imagePath)
        {
            Sprite sprite;

            if (!File.Exists(imagePath))
                return null;

            var fileData = File.ReadAllBytes(imagePath);

            // Try to load the image data into a sprite


            var texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            texture.filterMode = FilterMode.Trilinear;
            texture.anisoLevel = 9;
            texture.mipMapBias = (float) -0.5;
            texture.Apply();
            sprite = Sprite.Create(
                texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }

        public void SwapModsInLoadOrderAndPersistToFile(int thisModIndex, int swapWithModIndex)
        {

            if (thisModIndex < 0 || thisModIndex >= _enabledModsLoadOrder.Count)
            {
                NoonUtility.LogWarning($"Trying to swap enabled mods at position {thisModIndex} and {swapWithModIndex}, but {thisModIndex} isn't valid");
                return;
            }

            if (swapWithModIndex < 0 || swapWithModIndex >= _enabledModsLoadOrder.Count)
            {
                NoonUtility.LogWarning($"Trying to swap enabled mods at position {thisModIndex} and {swapWithModIndex}, but {swapWithModIndex} isn't valid");
                return;
            }


            

            (_enabledModsLoadOrder[thisModIndex], _enabledModsLoadOrder[swapWithModIndex]) = (_enabledModsLoadOrder[swapWithModIndex], _enabledModsLoadOrder[thisModIndex]);
            PersistEnabledModsLoadOrderToFile();
        }
    }
}
