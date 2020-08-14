using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Assets.Core.Fucine;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure.Modding
{
    /// <summary>
    /// Handles location, enablement and manifest verification of mods
    /// Actual entity loading is handled by the entity data loading classes
    /// </summary>
    public class ModManager
    {
        private const string MOD_MANIFEST_FILE_NAME = "synopsis.json";

        private static readonly string LocalModsPath = Path.Combine(Application.persistentDataPath, "mods");

        

        private static readonly string ModEnabledListPath = Path.Combine(Application.persistentDataPath, "mods.txt");

        private Dictionary<string, Mod> _cataloguedMods { get; }


        /// <summary>
        /// TODO: base this on importable type tags
        /// </summary>
        private readonly HashSet<string> _imagesDirectories = new HashSet<string>
        {
            "aspects",
            "burns",
            "cardbacks",
            "elements",
            "elements\\anim",
            "endings",
            "legacies",
            "ui",
            "verbs",
            "verbs\\anim"
        };



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

            var storefrontServicesProvider = Registry.Retrieve<StorefrontServicesProvider>();

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
                    NoonUtility.Log($"Duplicate mod id {sm.Key} - not cataloguing duplicate instance of mod");
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
                            messageLevel: 1);
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
                                messageLevel: 1);
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
            var modId = Path.GetFileName(modFolder);
            if (modId == null)
            {
                NoonUtility.Log("Unexpected null directory name for mod");
                continue;
            }

            NoonUtility.Log("Found directory for mod " + modId,0,VerbosityLevel.SystemChatter);

            // Find the mod's manifest and load its data
            var manifestPath = Path.Combine(modFolder, MOD_MANIFEST_FILE_NAME);
            if (!File.Exists(manifestPath))
            {
                NoonUtility.Log(
                    "Mod synopsis not found; skipping mod",
                    messageLevel: 2);
                continue;
            }

            //TODO: refactor to JSON.NET
            var manifestData = SimpleJsonImporter.Import(File.ReadAllText(manifestPath));
            if (manifestData == null)
            {
                NoonUtility.Log(
                    "Invalid mod manifest JSON; skipping mod",
                    messageLevel: 2);
                continue;
            }

            // Initialize the mod with its manifest information
            var mod = new Mod(modId, modFolder);

            var errors = mod.FromManifest(manifestData);
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    NoonUtility.Log(error, messageLevel: 2);
                }

                NoonUtility.Log(
                    "Encountered errors in manifest; skipping mod",
                    messageLevel: 2);
                continue;
            }

            var candidateContentFolder = Path.Combine(modFolder, NoonConstants.CONTENT_FOLDER_NAME);
            if (!Directory.Exists(candidateContentFolder))
            {
                NoonUtility.Log(
                    mod.Id + " has no content directory", 0, VerbosityLevel.SystemChatter);
            }
            else
                mod.ContentFolder = candidateContentFolder;

            var candidateLocFolder = Path.Combine(modFolder, NoonConstants.LOC_FOLDER_NAME);
            if (!Directory.Exists(candidateContentFolder))
            {
                NoonUtility.Log(mod.Id + " has no loc directory", 0, VerbosityLevel.SystemChatter);
            }
            else
            {
                NoonUtility.Log(mod.Id + " has a loc directory", 0, VerbosityLevel.SystemChatter);
                mod.LocFolder = candidateLocFolder;
            }
                // Collect the mod's images
                // If an error occurs in the process, discard the mod
                //commented out - not checking until we load the mod
                //can we have image-only mods? in this case we will need to reconsider the content directory doesn't exist exclusion above
                if (!LoadAllImagesDirectory(mod, modFolder, "images\\"))
                {
                    NoonUtility.Log(
                        "Encountered errors in images, skipping mod",
                        messageLevel: 2);
                    continue;
                }

                // Add the mod to the collection
                mod.ModInstallType = modInstallTypeForLocation;

                NoonUtility.Log("Catalogued mod '" + modId + "'", 0, VerbosityLevel.SystemChatter);
            theseMods.Add(modId, mod);
        }

        return theseMods;
    }

    public Mod SetModEnableStateAndReloadContent(string modId, bool enable)
    {
 
            if (!_cataloguedMods.ContainsKey(modId))
                return null;


            _cataloguedMods[modId].Enabled = enable;
            SaveEnabledModList();

            
            var compendiumLoader = new CompendiumLoader();
            var existingCompendium = Registry.Retrieve<ICompendium>();
            compendiumLoader.PopulateCompendium(existingCompendium, Registry.Retrieve<Concursum>().GetCurrentCultureId());
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


        private bool LoadAllImagesDirectory(Mod mod, string modPath, string imagesFolder)
        {
            // Search all subdirectories for more image files
            return _imagesDirectories.All(
                imageSubDirectoryPath => LoadImages(mod, modPath,imagesFolder, imageSubDirectoryPath));
        }

        private static bool LoadImages(Mod mod, string modPath,string imagesFolder, string imagesSubdirectory)
        {
            Sprite previewSprite = LoadSprite(mod.PreviewImageFilePath);
            mod.PreviewImage = previewSprite;


            // Check if the directory exists, otherwise don't try looking for images in it
            var imagesSubdirectoryPath = Path.Combine(modPath,imagesFolder, imagesSubdirectory);
            if (!Directory.Exists(imagesSubdirectoryPath))
            {
                return true;
            }

            // Load all PNG images into memory
            // This may incur a performance hit - a better system may be needed later
            foreach (var imagePath in Directory.GetFiles(imagesSubdirectoryPath, "*.png"))
            {
                Sprite eachSprite;

                var fileResourceName = Path.Combine(imagesFolder, imagesSubdirectory, Path.GetFileNameWithoutExtension(imagePath));
                try
                {
                    eachSprite = LoadSprite(imagePath);
                }
                catch
                {
                    NoonUtility.Log(
                        "Invalid image file '" + fileResourceName + "'", 
                        2);
                    return false;
                }

                mod.Images.Add(fileResourceName, eachSprite);
            }
            return true;
        }

        private static Sprite LoadSprite(string imagePath)
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
