using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Assets.Core.Fucine;
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
        private const string MOD_MANIFEST_FILE_NAME = "manifest.json";

        private static readonly string AllModsPath = Path.Combine(Application.persistentDataPath, "mods");

        private static readonly string ModEnabledListPath = Path.Combine(Application.persistentDataPath, "mods.txt");

        private Dictionary<string, Mod> _mods { get; }


        /// <summary>
        /// TODO: base this on importable type tags
        /// </summary>
        private readonly HashSet<string> _imagesDirectories = new HashSet<string>
        {
            "burnImages/",
            "cardBacks/",
            "elementArt/",
            "elementArt/anim/",
            "endingArt/",
            "icons40/aspects/",
            "icons100/legacies/",
            "icons100/verbs/",
        };



        public ModManager()
        {
            _mods = new Dictionary<string, Mod>();
        }

        public IEnumerable<Mod> GetAllCataloguedMods()
        {
            var cataloguedMods = _mods.Values;
            return cataloguedMods;
        }

        public IEnumerable<Mod> GetAllActiveMods()
        {
            var activeMods = _mods.Values.Where(m => m.Enabled);
            return activeMods;
        }

    public void CatalogueMods()
        {
            //TODO: We need some refactoring here. We want to use the data loading code from EntityTypeDataLoader
            _mods.Clear();

            // Check if the mods folder exists
            if (!Directory.Exists(AllModsPath))
            {
                Directory.CreateDirectory(AllModsPath);
                NoonUtility.Log($"Mods folder not found, creating it at {AllModsPath}", messageLevel: 1);
            }

            // Load the mod data from the file system
            foreach (var modFolder in Directory.GetDirectories(AllModsPath))
            {
                var modId = Path.GetFileName(modFolder);
                if (modId == null)
                {
                    NoonUtility.Log("Unexpected null directory name for mod");
                    continue;
                }
                NoonUtility.Log("Found directory for mod " + modId);

                // Find the mod's manifest and load its data
                var manifestPath = Path.Combine(modFolder, MOD_MANIFEST_FILE_NAME);
                if (!File.Exists(manifestPath))
                {
                    NoonUtility.Log(
                        "Mod manifest not found, skipping mod",
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
                var mod = new Mod(modId);
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

                //check the mod has a content directory
               mod.ContentFolder= Path.Combine(modFolder, NoonConstants.CONTENT_FOLDER_NAME);
                if(!Directory.Exists(mod.ContentFolder))
                {
                    NoonUtility.Log(
                        mod.Id + " doesn't have a content directory; skipping mod",
                        messageLevel: 2);
                    continue;
                }

                // Collect the mod's images
                // If an error occurs in the process, discard the mod
                //commented out - not checking until we load the mod
                //can we have image-only mods? in this case we will need to reconsider the content directory doesn't exist exclusion above
                if (!LoadAllImagesDirectory(mod, Path.Combine(modFolder, "images")))
                {
                    NoonUtility.Log(
                        "Encountered errors in images, skipping mod",
                        messageLevel: 2);
                    continue;
                }

                // Add the mod to the collection


                NoonUtility.Log("Catalogued mod '" + modId + "'");
                _mods.Add(modId, mod);
            }
            

            // Check the dependencies to see if there are any missing or invalid ones
            foreach (var mod in _mods)
            {
                foreach (var dependency in mod.Value.Dependencies)
                {
                    if (!_mods.ContainsKey(dependency.ModId))
                    {
                        NoonUtility.Log(
                            "Dependency '" + dependency.ModId + "' for '" + mod.Key + "' not found ", 
                            messageLevel: 1);
                    }
                    else
                    {
                        var availableVersion = _mods[dependency.ModId].Version;
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
                if (_mods.ContainsKey(modId))
                    _mods[modId].Enabled = true;
            }
        }

        public void SetModEnableState(string modId, bool enable)
        {
            if (!_mods.ContainsKey(modId))
                return;

            _mods[modId].Enabled = enable;
            SaveEnabledModList();
        }

        private static IEnumerable<string> LoadEnabledModList()
        {
            return File.Exists(ModEnabledListPath) ? File.ReadAllText(ModEnabledListPath).Split('\n') : new string[] {};
        }

        private void SaveEnabledModList()
        {
            File.WriteAllText(
                ModEnabledListPath, 
                string.Join("\n", _mods.Values.Where(m => m.Enabled).Select(m => m.Id).ToArray()));
        }

       
        public Sprite GetSprite(string spriteResourceName)
        {
            
            foreach (var mod in _mods.Values)
            {
                if (mod.Enabled && mod.Images.ContainsKey(spriteResourceName))
                {
                    return mod.Images[spriteResourceName];
                }
            }

            return null;
        }


        private bool LoadAllImagesDirectory(Mod mod, string imagesDirectoryPath)
        {
            // Search all subdirectories for more image files
            return _imagesDirectories.All(
                imageSubDirectoryPath => LoadImagesDirectory(mod, imagesDirectoryPath, imageSubDirectoryPath));
        }

        private static bool LoadImagesDirectory(Mod mod, string imagesDirectoryPath, string imagesSubdirectory)
        {
            // Check if the directory exists, otherwise don't try looking for images in it
            var imagesSubdirectoryPath = Path.Combine(imagesDirectoryPath, imagesSubdirectory);
            if (!Directory.Exists(imagesSubdirectoryPath))
            {
                return true;
            }

            // Load all PNG images into memory
            // This may incur a performance hit - a better system may be needed later
            foreach (var imagePath in Directory.GetFiles(imagesSubdirectoryPath, "*.png"))
            {
                var fileResourceName = imagesSubdirectory + Path.GetFileNameWithoutExtension(imagePath);
                NoonUtility.Log("Loading image '" + fileResourceName + "'");
                var fileData = File.ReadAllBytes(imagePath);

                // Try to load the image data into a sprite
                Sprite sprite;
                try
                {
                    var texture = new Texture2D(2, 2);
                    texture.LoadImage(fileData);
                    texture.filterMode = FilterMode.Trilinear;
                    texture.anisoLevel = 9;
                    texture.mipMapBias = (float) -0.5;
                    texture.Apply();
                    sprite = Sprite.Create(
                        texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                catch
                {
                    NoonUtility.Log(
                        "Invalid image file '" + fileResourceName + "'", 
                        messageLevel: 2);
                    return false;
                }

                mod.Images.Add(fileResourceName, sprite);
            }
            return true;
        }
    }
}
