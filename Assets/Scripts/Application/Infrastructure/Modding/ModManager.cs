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
        private const string MOD_DLL_NAME= "main.dll";
        private const string MOD_INITIALISER_METHOD_NAME = "Initialise";

        private bool AssemblyLoadingForModsAttempted = false;


        private static readonly string LocalModsPath = Path.Combine(Application.persistentDataPath, "mods");

        

        private static readonly string ModEnabledListPath = Path.Combine(Application.persistentDataPath, "mods.txt");

        private Dictionary<string, Mod> _cataloguedMods { get; }

        public void LoadModDLLs()
        {

            if (AssemblyLoadingForModsAttempted)
            {
                NoonUtility.LogWarning("Tried to load assemblies for mods twice. We don't want that.");
                return;
            }

            AssemblyLoadingForModsAttempted = true;

            foreach (Mod mod in Watchman.Get<ModManager>().GetEnabledMods())
                TryLoadDllsForMod(mod);
        }

        private void TryLoadDllsForMod(Mod mod)
        {
            string dll_path = Path.Combine(mod.ModRootFolder, MOD_DLL_FOLDER, MOD_DLL_NAME);

            if (!File.Exists(dll_path)) //no mod found, return
                return;

            Assembly modAssembly;

            try
            {
                modAssembly = Assembly.LoadFrom(dll_path);
            }
            catch (Exception cantLoadException)
            {
                NoonUtility.LogWarning($"Can't load a valid assembly at {dll_path}: {cantLoadException.Message}");
                return;
            }

            NoonUtility.Log($"Loaded {dll_path} for {mod.Name}");

            Type modInitialiserType;
            string mod_initialiser_class_name=string.Empty;

            try
            {
                 mod_initialiser_class_name= Regex.Replace(mod.Name, "[^a-zA-Z0-9_]+", "");
                modInitialiserType = modAssembly.GetType(mod_initialiser_class_name);
            }
            catch (Exception e)
            {
                NoonUtility.LogWarning($"Can't find a class named {mod_initialiser_class_name} in dll at {dll_path}: {e.Message}");
                return;
            }


            if(modInitialiserType==null)
                NoonUtility.LogWarning($"Tried to get a class type named {mod_initialiser_class_name} in dll at {dll_path}, but it's coming back as null");


            MethodInfo initialiserMethod;

            try
            {
                initialiserMethod = modInitialiserType.GetMethod(MOD_INITIALISER_METHOD_NAME);
                initialiserMethod.Invoke(null, null);
            }
            catch (Exception e)
            {
                NoonUtility.LogWarning($"Tried to invoke {MOD_INITIALISER_METHOD_NAME}() on a type named {mod_initialiser_class_name} in dll at {dll_path}, but failed: {e.Message}");
                return;
            }

            NoonUtility.Log($"Successfully invoked {MOD_INITIALISER_METHOD_NAME}()on a type named {mod_initialiser_class_name} in dll at {dll_path}");

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

        public IEnumerable<Mod> GetEnabledMods()
        {
            var activeMods = _cataloguedMods.Values.Where(m => m.Enabled);
            return activeMods;
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
             
            // Enable all mods that have been marked as enabled
            foreach (var modId in LoadEnabledModList())
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
                return null;


            _cataloguedMods[modId].Enabled = enable;
            SaveEnabledModList();

            
            var compendiumLoader = new CompendiumLoader(Watchman.Get<Config>().GetConfigValue(NoonConstants.CONTENT_FOLDER_NAME_KEY));
            var existingCompendium = Watchman.Get<Compendium>();
            compendiumLoader.PopulateCompendium(existingCompendium, Watchman.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY));
            var modToAlter= _cataloguedMods[modId];

       

            return modToAlter;
        }

        private static IEnumerable<string> LoadEnabledModList()
        {
            return File.Exists(ModEnabledListPath) ? File.ReadAllText(ModEnabledListPath).Split('\n') : new string[] {};
        }

        private void SaveEnabledModList()
        {
            File.WriteAllText(
                ModEnabledListPath, 
                string.Join("\n", _cataloguedMods.Values.Where(m => m.Enabled).Select(m => m.Id).ToArray()));
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

            var imagesFolderForMod = Path.Combine(modPath, "images\\");
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
    }
}
