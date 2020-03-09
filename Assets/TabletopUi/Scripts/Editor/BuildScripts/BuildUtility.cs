using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Editor;
using Assets.TabletopUi.Scripts.Editor.BuildScripts;
using Galaxy;
using Noon;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Storefront = TabletopUi.Scripts.Services.Storefront;

namespace Assets.Core.Utility
{

    public static class BuildUtility
    {

        private const string BUILD_DIR_PREFIX = "csunity-";
        private const string DEFAULT_BUILD_DIR_ROOT = "G:\\cs\\build_outputs\\BASE_OS_BUILDS";
        private const string CONST_DLC_FOLDER = "DLC";
        private const string CONST_STOREFRONTS = "STOREFRONT_DISTRIBUTIONS";
        private const string CONST_PERPETUALEDITION_DLCTITLE = "PERPETUAL";
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

        [MenuItem("Tools/Build (All)")]
        public static void PerformAllBuilds()
        {
            PerformWindowsBuild();
            PerformOsxBuild();
            PerformLinuxBuild();
        }

        [MenuItem("Tools/Build (Windows)")]
        public static void PerformWindowsBuild()
        {
            PerformBuild(BuildTarget.StandaloneWindows);
        }

        [MenuItem("Tools/Build (OSX)")]
        public static void PerformOsxBuild()
        {
            PerformBuild(BuildTarget.StandaloneOSX);
        }

        [MenuItem("Tools/Build (Linux)")]
        public static void PerformLinuxBuild()
        {
            PerformBuild(BuildTarget.StandaloneLinux64);
        }

        [MenuItem("Tools/Make Distribution (Steam)")]
        public static void MakeSteamDistribution()
        {
            MakeDistribution(Storefront.Steam);

            }

        private static void PerformBuild(BuildTarget target)
        {

            // Clear the build directory of any of the intermediate results of a previous build
            DirectoryInfo rootDir = new DirectoryInfo(DEFAULT_BUILD_DIR_ROOT);
            foreach (var file in rootDir.GetFiles())
                File.Delete(file.FullName);
            foreach (var directory in rootDir.GetDirectories())
                Directory.Delete(directory.FullName, true);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                target = target,
                locationPathName = NoonUtility.JoinPaths(GetEditionSpecificBuildDirectory(DEFAULT_BUILD_DIR_ROOT,Product.VANILLA,target), GetExeNameForTarget(target)),
                scenes = Scenes
            };
            Log("Building " + target + " version to build directory: " + buildPlayerOptions.locationPathName);

            BuildPipeline.BuildPlayer(buildPlayerOptions);
            
        }

        private static string GetOsSpecificBuildDirectory(string root,BuildTarget target)
        {
            return NoonUtility.JoinPaths(DEFAULT_BUILD_DIR_ROOT, GetPlatformFolderForTarget(target));
        }

        private static string GetEditionSpecificBuildDirectory(string root,Product product, BuildTarget target)
        {
            var osBuildPath = GetOsSpecificBuildDirectory(root, target);

            return NoonUtility.JoinPaths(osBuildPath, product.ToString());
        }

        [PostProcessBuild]
        public static void OnBuildComplete(BuildTarget target, string pathToBuiltProject)
        {
          //  BuildEnvironment environment=

            if (target != BuildTarget.StandaloneWindows
                && target != BuildTarget.StandaloneWindows64
                && target != BuildTarget.StandaloneOSX
                && target != BuildTarget.StandaloneLinux64)
            {
                return;
            }

            PostBuildFileTasks(target, GetParentDirectory(pathToBuiltProject), Path.GetFileName(pathToBuiltProject));
        }

        private static void PostBuildFileTasks(BuildTarget buildTarget, string pathToBuiltProject, string exeName)
        {

            CopyStorefrontLibraries(buildTarget, pathToBuiltProject);
            AddVersionNumber(pathToBuiltProject);

            string perpetualEditionsFolderPath =NoonUtility.JoinPaths(GetOsSpecificBuildDirectory(DEFAULT_BUILD_DIR_ROOT,buildTarget), Product.PERPETUAL_ALLDLC.ToString());
            BuildPerpetualEdition(pathToBuiltProject,perpetualEditionsFolderPath);

            string DLCFolderPath=NoonUtility.JoinPaths(GetOsSpecificBuildDirectory(DEFAULT_BUILD_DIR_ROOT,buildTarget), CONST_DLC_FOLDER);

            ExtractDLCFilesFromBaseBuilds(pathToBuiltProject,DLCFolderPath,buildTarget, exeName);

          //  RunContentTests(buildTarget, builtAtPath);



        }

        private static void MakeDistribution(Storefront storefront)
        {
            BuildEnvironment env = new BuildEnvironment(DEFAULT_BUILD_DIR_ROOT);

            List<BuildOS> OSs=new List<BuildOS>();
            List<BuildProduct> products=new List<BuildProduct>();

            BuildOS osw=new BuildOS(OSId.Windows);
            BuildOS oso=new BuildOS(OSId.OSX);
            BuildOS osl=new BuildOS(OSId.Linux);
            OSs.Add(osw);
            OSs.Add(oso);
            OSs.Add(osl);


            BuildProduct vanillaEdition=new BuildProduct(env,Product.VANILLA,false);
            BuildProduct perpetualEdition=new BuildProduct(env,Product.PERPETUAL_ALLDLC,false);
            BuildProduct perpetualDLC=new BuildProduct(env,Product.PERPETUAL,true);
            BuildProduct DancerDLC=new BuildProduct(env,Product.DANCER,true);
            BuildProduct PriestDLC=new BuildProduct(env,Product.PRIEST,true);
            BuildProduct GhoulDLC=new BuildProduct(env,Product.GHOUL,true);
            BuildProduct ExileDLC=new BuildProduct(env,Product.EXILE,true);

            products.Add(vanillaEdition);
            products.Add(perpetualEdition);
            products.Add(perpetualDLC);
            products.Add(DancerDLC);
            products.Add(PriestDLC);
            products.Add(GhoulDLC);
            products.Add(ExileDLC);


            BuildStorefront buildStorefront=new BuildStorefront(storefront,OSs,products);

            var distributions = buildStorefront.GetDistributionsForStorefront(env);

            foreach(var d in distributions)
                d.CopyFilesFromEnvironment(env);


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

        private static void BuildPerpetualEdition(string from,string to)
        {
            // Set up the Perpetual Edition, with all its DLC

            Log("Copying whole project with DLC from " + from + " to " + to);

            if (Directory.Exists(to))
                Directory.Delete(to, true);
            NoonUtility.CopyDirectoryRecursively(from, to);
        }


        private static void ExtractDLCFilesFromBaseBuilds(string from,string to,BuildTarget buildTarget, string exeName)
        {
// Take the DLCs out of the base edition and into their own directories

   
            foreach (var dlcContentType in ContentTypes)
            foreach (var locale in Locales)
                MoveDlcContent(from, to, buildTarget, exeName, dlcContentType, locale);
        }



        private static void CopyStorefrontLibraries(BuildTarget target, string builtAtPath)
        {
            Log("Copying Steam libraries to build: " + builtAtPath);
            CopySteamLibraries.Copy(target, builtAtPath);
            Log("Copying Galaxy libraries to build: " + builtAtPath);
            CopyGalaxyLibraries.Copy(target, builtAtPath);
        }

        private static void MoveDlcContent(string from, string to, BuildTarget target, string exeName, string contentOfType, string locale)
        {
            var baseEditionContentPath = GetCoreContentPath(from, exeName, contentOfType, locale);
            Log("Searching for DLC in " + baseEditionContentPath);

            var contentFiles = Directory.GetFiles(baseEditionContentPath).ToList().Where(f => f.EndsWith(".json"));
            foreach (var contentFilePath in contentFiles)
            {
                int dlcMarkerIndex = contentFilePath.IndexOf(CONST_DLC_FOLDER + CONST_NAME_SEPARATOR_CHAR, StringComparison.Ordinal);

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
                    NoonUtility.JoinPaths(to, dlcTitle, GetPlatformFolderForTarget(target)), exeName, contentOfType, locale);

                string dlcFileDestinationPath = NoonUtility.JoinPaths(dlcDestinationDir, dlcFilenameWithoutPath);
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
            string semperPath = NoonUtility.JoinPaths(
                to,
                CONST_PERPETUALEDITION_DLCTITLE,
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



        private static void AddVersionNumber(string exeFolder)
        {
            string versionPath = NoonUtility.JoinPaths(exeFolder, "version.txt");
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
            return NoonUtility.JoinPaths(
                basePath,
                GetDataFolderForTarget(exeName),
                CONST_CORE_CONTENT_LOCATION + (locale != null ? "_" + locale : ""),
                contentOfType);
        }


        public static void Log(string message)
        {
            var oldStackTraceLogType = Application.GetStackTraceLogType(LogType.Log);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log("CS>>>>> " + message);
            Application.SetStackTraceLogType(LogType.Log, oldStackTraceLogType);
        }



        private static string GetParentDirectory(string path)
        {
            return Path.GetFullPath(Path.Combine(path, @"../"));
        }

 

    }
}
