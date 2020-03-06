using System;
using System.IO;
using System.Linq;
using Assets.Editor;
using Galaxy;
using Noon;
using TabletopUi.Scripts.Services;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Assets.Core.Utility
{
    public static class BuildUtility
    {
        private const string BUILD_DIR_PREFIX = "csunity-";
        private const string DEFAULT_BUILD_DIR = "build";
        private const string CONST_DLC = "DLC";
        private const string CONST_STOREFRONTS = "STOREFRONT_DISTRIBUTIONS";
        private const string CONST_PERPETUALEDITIONLOCATION = "PERPETUAL_ALLDLC";
        private const string CONST_PERPETUALEDITION_DLC = "PERPETUAL";
        private const string CONST_PERPETUALEDITION_SEMPER_RELATIVE_PATH_TO_FILE = "StreamingAssets/edition/semper.txt";
        private const string CONST_STOREFRONT_RELATIVE_PATH_TO_FILE = "StreamingAssets/edition/store.txt";
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
            PerformBuild(BuildTarget.StandaloneLinux64, "Linux");
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
            Log("Building " + label + " version to build directory: " + buildPlayerOptions.locationPathName);

            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
        
        [PostProcessBuild]
        public static void OnBuildComplete(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.StandaloneWindows
                && target != BuildTarget.StandaloneWindows64
                && target != BuildTarget.StandaloneOSX
                && target != BuildTarget.StandaloneLinux64)
            {
                return;
            }

            PostBuildFileTasks(target, Directory.GetParent(pathToBuiltProject).FullName, Path.GetFileName(pathToBuiltProject));
        }

        private static void PostBuildFileTasks(BuildTarget buildTarget, string builtAtPath, string exeName)
        {
            // For CI, we moved the build output into a build- and platform-specific subdirectory
            //Now I'm back to deploying from a local machine, I no longer do this, because it's not straightforward to 
            //var outputFolder = BUILD_DIR_PREFIX + NoonUtility.VersionNumber;
            //var buildPath = JoinPaths(builtAtPath, outputFolder);
            //var platformDirName = GetPlatformFolderForTarget(buildTarget);
            //var baseEditionPath = JoinPaths(buildPath, platformDirName);
            //if (Directory.Exists(baseEditionPath))
            //{
            //    Log("Deleting base edition path " + baseEditionPath +" because it already exists");
            //    Directory.Delete(baseEditionPath, true);
            //}

           // Log("Copying root path (" + builtAtPath + ") contents to base edition path (" + baseEditionPath + ")");
            //CopyDirectoryRecursively(builtAtPath, baseEditionPath);
            
       

           // WriteStoreFile()

            AddVersionNumber(builtAtPath);
            
            BuildPerpetualEdition(buildTarget,builtAtPath,exeName);

            ExtractDLCs(builtAtPath,buildTarget, exeName);

          //  RunContentTests(buildTarget, builtAtPath);


            // Copy APIs/integration for storefronts
            CopyStorefrontLibraries(buildTarget, builtAtPath);


            BakeDistribution(buildTarget, builtAtPath,exeName,Storefront.Steam);
            BakeDistribution(buildTarget, builtAtPath,exeName,Storefront.Gog);
            BakeDistribution(buildTarget, builtAtPath,exeName,Storefront.Humble);
            BakeDistribution(buildTarget, builtAtPath,exeName,Storefront.Itch);

            
            //do all this again for perpetual
        }


        private static void BakeDistribution(BuildTarget buildTarget, string builtAtPath,string exeName,Storefront storefront)
        {
            var storefrontsPath = JoinPaths(GetGrandfatherPath(builtAtPath), CONST_STOREFRONTS);
            var distributionDir = JoinPaths(storefrontsPath, storefront.ToString());
            var osDirForDistribution = JoinPaths(distributionDir, GetPlatformFolderForTarget(buildTarget));
            
            Log("Removing old " + storefront + " distribution directory: " + osDirForDistribution);
            Directory.Delete(osDirForDistribution, true);
            
            Log("Creating new " + storefront + " distribution directory: " + osDirForDistribution);
            Directory.CreateDirectory(osDirForDistribution);

            CopyDirectoryRecursively(builtAtPath, osDirForDistribution);

            var storefrontFilePath = JoinPaths(osDirForDistribution,
                GetDataFolderForTarget(exeName),
                    CONST_STOREFRONT_RELATIVE_PATH_TO_FILE);

            Log("Writing storefront info to " + storefrontFilePath);

            File.WriteAllText(storefrontFilePath, storefront.ToString());
        }






        private static void RunContentTests(BuildTarget buildTarget, string builtAtPath)
        {
// Run the content tests, but only for Windows, since otherwise we'll end up with three copies
            if (buildTarget == BuildTarget.StandaloneWindows || buildTarget == BuildTarget.StandaloneWindows64)
            {
                ContentTester.ValidateContentAssertions(false);
                FileUtil.ReplaceFile(ContentTester.JUnitResultsPath, Path.Combine(builtAtPath, "csunity-tests.xml"));
            }
        }

        private static string BuildPerpetualEdition(BuildTarget buildTarget,string builtAtPath,string exeName)
        {
            // Set up the Perpetual Edition, with all its DLC
            ;
            string perpetualEditionForPlatformPath = JoinPaths(GetGrandfatherPath(builtAtPath), CONST_PERPETUALEDITIONLOCATION, GetPlatformFolderForTarget(buildTarget));

            Log("Copying whole project with DLC from " + builtAtPath + " to " + perpetualEditionForPlatformPath);

            if (Directory.Exists(perpetualEditionForPlatformPath))
                Directory.Delete(perpetualEditionForPlatformPath, true);
            CopyDirectoryRecursively(builtAtPath, perpetualEditionForPlatformPath);


            // Create the Perpetual Edition DLC
            string semperPath = JoinPaths(
                perpetualEditionForPlatformPath,
                GetDataFolderForTarget(exeName),
                CONST_PERPETUALEDITION_SEMPER_RELATIVE_PATH_TO_FILE);

            WriteSemperFile(semperPath);

            return perpetualEditionForPlatformPath;
        }

        private static void ExtractDLCs(string builtAtPath,BuildTarget buildTarget, string exeName)
        {
// Take the DLCs out of the base edition and into their own directories
            var dlcPath = JoinPaths(GetGrandfatherPath(builtAtPath), CONST_DLC);
            foreach (var dlcContentType in ContentTypes)
            foreach (var locale in Locales)
                MoveDlcContent(builtAtPath, dlcPath, buildTarget, exeName, dlcContentType, locale);
        }

 

        private static void CopyStorefrontLibraries(BuildTarget target, string builtAtPath)
        {
            Log("Copying Steam libraries to build: " + builtAtPath);
            CopySteamLibraries.Copy(target, builtAtPath);
            Log("Copying Galaxy libraries to build: " + builtAtPath);
            CopyGalaxyLibraries.Copy(target, builtAtPath);
        }

        private static void MoveDlcContent(string builtAtPath, string dlcPath, BuildTarget target, string exeName, string contentOfType, string locale)
        {
            var baseEditionContentPath = GetCoreContentPath(builtAtPath, exeName, contentOfType, locale);
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
                    JoinPaths(dlcPath, dlcTitle, GetPlatformFolderForTarget(target)), exeName, contentOfType, locale);

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
                GetDataFolderForTarget(exeName),
                CONST_PERPETUALEDITION_SEMPER_RELATIVE_PATH_TO_FILE);

            WriteSemperFile(semperPath);
        }

        private static void WriteSemperFile(string semperPath)
        {
            //semper.txt is a dumbfile that activates the PERPETUAL EDITION menu banner.
            //This needs to be created as a tiny piece of DLC, but also injected into the /edition folder of the Perpetual Edition build.

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
                case BuildTarget.StandaloneWindows64:
                    return "cultistsimulator.exe";
                case BuildTarget.StandaloneOSX:
                    return "OSX.app";
                case BuildTarget.StandaloneLinux64:
                    return "CS.x86";
                default:
                    throw new ApplicationException("We don't know how to handle this build buildTarget: " + target);
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
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                case BuildTarget.StandaloneLinux64:
                    return "Linux";
                default:
                    throw new ApplicationException("We don't know how to handle this build buildTarget: " + target);
            }
        }

        private static string GetCoreContentPath(string basePath, string exeName, string contentOfType, string locale)
        {
            return JoinPaths(
                basePath, 
                GetDataFolderForTarget(exeName), 
                CONST_CORE_CONTENT_LOCATION + (locale != null ? "_" + locale : ""), 
                contentOfType);
        }

        private static bool IsPermittedFileToCopy(FileSystemInfo file)
        {
            return file.Name != ".dropbox";
        }

        public static void Log(string message)
        {
            var oldStackTraceLogType = Application.GetStackTraceLogType(LogType.Log);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log("CS>>>>> " + message);
            Application.SetStackTraceLogType(LogType.Log, oldStackTraceLogType);
        }

        private static string JoinPaths(params string[] paths)
        {
            return paths.Aggregate("",Path.Combine);
        }

        private static string GetParentDirectory(string path)
        {
            return Path.GetFullPath(Path.Combine(path, @"../"));
        }

        private static string GetGrandfatherPath(string path)
        {
            return GetParentDirectory(GetParentDirectory(path));
        }

    }
}
