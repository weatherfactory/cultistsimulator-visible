using System;
using System.IO;
using System.Linq;
using Facepunch.Editor;
using Galaxy;
using Noon;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Assets.Core.Utility
{
    public static class BuildUtility
    {
        private const string BUILD_DIR_PREFIX = "csunity-";
        private const string DEFAULT_BUILD_DIR = "build";
        private const string CONST_DLC = "DLC";
        private const string CONST_PERPETUALEDITIONLOCATION = "PERPETUAL_ALLDLC";
        private const string CONST_PERPETUALEDITION_DLC = "PERPETUAL";
        private const string CONST_PERPETUALEDITION_SEMPER_PATH = "StreamingAssets/edition/semper.txt";
        private const string CONST_CORE_CONTENT_LOCATION = "StreamingAssets/content/core";
        private const string CONST_DATA_FOLDER_SUFFIX = "_Data";
        private const char CONST_NAME_SEPARATOR_CHAR = '_';

        private static readonly string[] Scenes = 
        {
            "Assets/TabletopUi/Logo.unity",
            "Assets/TabletopUi/Quote.unity",
            "Assets/TabletopUi/Menu.unity",
            "Assets/TabletopUi/Tabletop.unity",
            "Assets/TabletopUi/GameOver.unity",
            "Assets/TabletopUi/NewGame.unity",
            "Assets/TabletopUi/Global.unity",
        };

        private static readonly string[] ContentTypes =
        {
            "decks",
            "elements",
            "endings",
            "legacies",
            "recipes",
            "verbs"
        };

        private static readonly string[] Locales =
        {
            null,  // Default locale (en)
            "ru",
            "zh-hans"
        };
        
        [MenuItem("Tools/Build (Windows)")]
        public static void PerformWindowsBuild()
        {
            PerformBuild(BuildTarget.StandaloneWindows, "Windows");
        }

        [MenuItem("Tools/Build (OSX)")]
        public static void PerformOsxBuild()
        {
            PerformBuild(BuildTarget.StandaloneOSX, "OSX");
        }

        [MenuItem("Tools/Build (Linux)")]
        public static void PerformLinuxBuild()
        {
            PerformBuild(BuildTarget.StandaloneLinuxUniversal, "Linux");
        }
        
        private static void PerformBuild(BuildTarget target, string label)
        {
            var args = Environment.GetCommandLineArgs();
            var buildRootPath = args.Length > 1 ? Environment.GetCommandLineArgs()[1] : DEFAULT_BUILD_DIR;
            
            // Clear the build directory of any of the intermediate results of a previous build
            // This excludes any existing build directories, so that we can easily combine builds for different
            // platforms and versions
            DirectoryInfo rootDir = new DirectoryInfo(buildRootPath);
            foreach (var file in rootDir.GetFiles())
                File.Delete(file.FullName);
            foreach (var directory in rootDir.GetDirectories().Where(d => !d.Name.StartsWith(BUILD_DIR_PREFIX)))
                Directory.Delete(directory.FullName, true);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                target = target,
                locationPathName = JoinPaths(buildRootPath, GetExeNameForTarget(target)),
                scenes = Scenes
            };
            Log("Building " + label + " version to " + buildPlayerOptions.locationPathName);

            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
        
        [PostProcessBuild]
        public static void OnBuildComplete(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.StandaloneWindows
                && target != BuildTarget.StandaloneOSX
                && target != BuildTarget.StandaloneLinuxUniversal)
            {
                return;
            }

            PostBuildFileTasks(target, Directory.GetParent(pathToBuiltProject).FullName);
        }

        private static void PostBuildFileTasks(BuildTarget target, string rootPath)
        {
            // Move the build output into its build- and platform-specific subdirectory
            var outputFolder = BUILD_DIR_PREFIX + NoonUtility.VersionNumber;
            var buildPath = JoinPaths(rootPath, outputFolder);
            var platformDirName = GetPlatformFolderForTarget(target);
            var baseEditionPath = JoinPaths(buildPath, platformDirName);
            if (Directory.Exists(baseEditionPath))
                Directory.Delete(baseEditionPath, true);
            CopyDirectoryRecursively(rootPath, baseEditionPath);
            
            // Copy some extra files directly from the Unity project
            Log("Copying Steam libraries");
            CopySteamLibraries.Copy(target, baseEditionPath);
            Log("Copying Galaxy libraries");
            CopyGalaxyLibraries.Copy(target, baseEditionPath);
            Log("Adding version number");
            AddVersionNumber(baseEditionPath);
            
            // Set up the Perpetual Edition, with all its DLC
            string perpetualEditionPath = JoinPaths(buildPath, CONST_PERPETUALEDITIONLOCATION, platformDirName);
            Log("Copying whole project with DLC from " + baseEditionPath + " to " + perpetualEditionPath);
            if (Directory.Exists(perpetualEditionPath))
                Directory.Delete(perpetualEditionPath, true);
            CopyDirectoryRecursively(baseEditionPath, perpetualEditionPath);
            
            // Take the DLCs out of the base edition and into their own directories
            var dlcPath = JoinPaths(buildPath, CONST_DLC);
            foreach (var dlcContentType in ContentTypes)
                foreach (var locale in Locales)
                    MoveDlcContent(baseEditionPath, dlcPath, target, dlcContentType, locale);
        }

        private static void MoveDlcContent(string baseEditionPath, string dlcPath, BuildTarget target, string contentOfType, string locale)
        {
            var baseEditionContentPath = GetCoreContentPath(baseEditionPath, target, contentOfType, locale);
            Log("Searching for DLC in " + baseEditionContentPath);
            
            var contentFiles = Directory.GetFiles(baseEditionContentPath).ToList().Where(f => f.EndsWith(".json"));
            foreach (var contentFilePath in contentFiles)
            {
                int dlcMarkerIndex = contentFilePath.IndexOf(CONST_DLC + CONST_NAME_SEPARATOR_CHAR, StringComparison.Ordinal);

                // Does it begin with "DLC_"?
                if (dlcMarkerIndex <= -1) 
                    continue;
                
                // Extract the DLC title so it can be moved to the appropriate subdirectory
                string dlcFilenameWithoutPath = contentFilePath.Substring(dlcMarkerIndex);
                Log("DLC file found: " + dlcFilenameWithoutPath);
                string dlcTitle = dlcFilenameWithoutPath.Split(CONST_NAME_SEPARATOR_CHAR)[1];
                if (string.IsNullOrEmpty(dlcTitle))
                    throw new ApplicationException("Couldn't find DLC title for file " + contentFilePath);

                //get the DLC location (../DLC/[title]/[platform]/[datafolder]/StreamingAssets/content/core/[contentOfType]/[thatfile]
                string dlcDestinationDir = GetCoreContentPath(
                    JoinPaths(dlcPath, dlcTitle, GetPlatformFolderForTarget(target)), target, contentOfType, locale);
                string dlcFileDestinationPath = JoinPaths(dlcDestinationDir, dlcFilenameWithoutPath);
                if (Directory.Exists(dlcDestinationDir))
                {
                    Directory.Delete(dlcDestinationDir, true);
                    Log("Removing old directory: " + dlcDestinationDir);
                }
                Log("Creating directory: " + dlcDestinationDir);
                Directory.CreateDirectory(dlcDestinationDir);

                Log("Moving file " + contentFilePath + " to " + dlcFileDestinationPath);
                File.Move(contentFilePath, dlcFileDestinationPath);
            }
            
            // Create the Perpetual Edition DLC
            string semperPath = JoinPaths(
                dlcPath,
                CONST_PERPETUALEDITION_DLC,
                GetPlatformFolderForTarget(target),
                GetDataFolderForTarget(GetExeNameForTarget(target)),
                CONST_PERPETUALEDITION_SEMPER_PATH);
            string semperDirPath = Path.GetDirectoryName(semperPath);
            if (semperDirPath != null && !Directory.Exists(semperDirPath))
            {
                Directory.CreateDirectory(semperDirPath);
            }
            File.WriteAllText(semperPath, string.Empty);
        }

        private static void CopyDirectoryRecursively(string source, string destination, bool move = false)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(source);
            DirectoryInfo destinationDirectory = new DirectoryInfo(destination);
            if (!destinationDirectory.Exists)
                destinationDirectory.Create();
            foreach (var file in sourceDirectory.GetFiles().Where(IsPermittedFileToCopy))
            {
                if (move)
                    file.MoveTo(JoinPaths(destination, file.Name));
                else
                    file.CopyTo(JoinPaths(destination, file.Name), true);
            }

            foreach (var directory in sourceDirectory.GetDirectories()
                .Where(d => !destinationDirectory.FullName.StartsWith(d.FullName)))
            {
                CopyDirectoryRecursively(directory.FullName, JoinPaths(destination, directory.Name), move);
                if (move)
                    Directory.Delete(directory.FullName);
            }
        }

        private static void AddVersionNumber(string exeFolder)
        {
            string versionPath = JoinPaths(exeFolder, "version.txt");
            Log("Writing version to " + versionPath);
            File.WriteAllText(versionPath, NoonUtility.VersionNumber.ToString());
        }

        private static string GetExeNameForTarget(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                    return "cultistsimulator.exe";
                case BuildTarget.StandaloneOSX:
                    return "OSX.app";
                case BuildTarget.StandaloneLinuxUniversal:
                    return "CS.x86";
                default:
                    throw new ApplicationException("We don't know how to handle this build target: " + target);
            }
        }

        private static string GetDataFolderForTarget(string exeName)
        {
            if (exeName.Contains("OSX")) // OSX is cray
                return "OSX.app/Contents/Resources/Data";
            return exeName.Split('.')[0] + CONST_DATA_FOLDER_SUFFIX;
        }

        private static string GetPlatformFolderForTarget(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                case BuildTarget.StandaloneLinuxUniversal:
                    return "Linux";
                default:
                    throw new ApplicationException("We don't know how to handle this build target: " + target);
            }
        }

        private static string GetCoreContentPath(string basePath, BuildTarget target, string contentOfType, string locale)
        {
            return JoinPaths(
                basePath, 
                GetDataFolderForTarget(GetExeNameForTarget(target)), 
                CONST_CORE_CONTENT_LOCATION + (locale != null ? "_" + locale : ""), 
                contentOfType);
        }

        private static bool IsPermittedFileToCopy(FileSystemInfo file)
        {
            return file.Name != ".dropbox";
        }

        private static void Log(string message)
        {
            Console.WriteLine(">>>>> " + message);
        }

        private static string JoinPaths(params string[] paths)
        {
            return paths.Aggregate("", Path.Combine);
        }
    }
}
