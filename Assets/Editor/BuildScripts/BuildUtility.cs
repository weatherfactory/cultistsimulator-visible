using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SecretHistories.Entities;
using SecretHistories.Editor.BuildScripts;
using Galaxy;
using SecretHistories.UI;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.WSA;
using Application = UnityEngine.Application;
using Storefront = SecretHistories.Enums.Storefront;
using SecretHistories.Enums;
using SecretHistories.Constants;

namespace SecretHistories.Utility
{

    public static class BuildUtility
    {

        private const string BUILD_DIR_PREFIX = "csunity-";
        private const string DEFAULT_BUILD_ROOT = "c:\\builds\\GAMEID\\build_outputs";
        private const string POST_BUILT_STREAMING_ASSETS_FOLDER = "StreamingAssets";
        private const string SCENES_FOLDER = "assets/Scenes";
        private const string CONTENT_FOLDER_CONTAINS_STRING = "content";
        private const string CONST_DLC_FOLDER = "DLC";
        private const string CONST_PERPETUALEDITION_DLCTITLE = "PERPETUAL";
        private const string CONST_PERPETUALEDITION_SEMPER_RELATIVE_PATH_TO_FILE = "StreamingAssets\\edition\\semper.txt";
        private const char CONST_NAME_SEPARATOR_CHAR = '_';


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
            "de",
            "jp",
            "ru",
            "zh-hans"
        };



        [MenuItem("Tools/CS Build (ALL)",false,10)]
        public static void PerformAllCSBuilds()
        {
            PerformBuild(Product.VANILLA, BuildTarget.StandaloneWindows);
            PerformBuild(Product.VANILLA, BuildTarget.StandaloneOSX);
            PerformBuild(Product.VANILLA, BuildTarget.StandaloneLinux64);
        }

        [MenuItem("Tools/CS Build (Windows)", false, 10)]
        public static void PerformWindowsBuild()
        {
            PerformBuild(Product.VANILLA, BuildTarget.StandaloneWindows);
        }

        [MenuItem("Tools/CS Build (OSX)", false, 10)]
        public static void PerformOsxBuild()
        {
            PerformBuild(Product.VANILLA, BuildTarget.StandaloneOSX);
        }

        [MenuItem("Tools/CS Build (Linux)", false, 10)]
        public static void PerformLinuxBuild()
        {
            PerformBuild(Product.VANILLA, BuildTarget.StandaloneLinux64);
        }

        //[MenuItem("Tools/BH Build (ALL)", false, 30)]
        //public static void PerformAllBHBuilds()
        //{
        //    PerformBuild(Product.BH, BuildTarget.StandaloneWindows);
        //    PerformBuild(Product.BH, BuildTarget.StandaloneOSX);
        //    PerformBuild(Product.BH, BuildTarget.StandaloneLinux64); <-- this is breaking; let's come back to it when we rationalise more. The data folder name is coming out as CS_Data somehow
        //}

        [MenuItem("Tools/BH Build (Windows)", false, 30)]
        public static void PerformWindowsBHBuild()
        {
            PerformBuild(Product.BH, BuildTarget.StandaloneWindows);
        }

        [MenuItem("Tools/BH Distribution (Steam)", false, 30)]
        public static void MakeBHSteamDistributionWindowsOnly()
        {

            BuildEnvironment env = new BuildEnvironment(GameId.BH, DEFAULT_BUILD_ROOT);

            List<BuildOS> OSs = new List<BuildOS>();
            List<BuildProduct> products = new List<BuildProduct>();

            BuildOS osw = new BuildOS(GameId.BH, BuildTarget.StandaloneWindows64);
            OSs.Add(osw);
            

            BuildProduct vanillaEdition = new BuildProduct(Product.BH, false);

            products.Add(vanillaEdition);

            BuildStorefront buildStorefront = new BuildStorefront(Storefront.Steam, OSs, products);

            MakeDistribution(GameId.BH, buildStorefront);


        }


        [MenuItem("Tools/CS Distribution (ALL)", false, 110)]
        public static void MakeAllDistributions()
        {
            MakeSteamDistribution();
            MakeGogDistribution();
            MakeHumbleDistribution();

        }

        [MenuItem("Tools/CS Distribution (Steam)", false, 110)]
        public static void MakeSteamDistribution()
        {
            
            BuildEnvironment env = new BuildEnvironment(GameId.CS, DEFAULT_BUILD_ROOT);

            List<BuildOS> OSs=new List<BuildOS>();
            List<BuildProduct> products=new List<BuildProduct>();

            BuildOS osw=new BuildOS(GameId.CS, BuildTarget.StandaloneWindows64);
            BuildOS oso=new BuildOS(GameId.CS, BuildTarget.StandaloneOSX);
            BuildOS osl=new BuildOS(GameId.CS, BuildTarget.StandaloneLinux64);
            OSs.Add(osw);
            OSs.Add(oso);
            OSs.Add(osl);


            BuildProduct vanillaEdition=new BuildProduct(Product.VANILLA,false);
            BuildProduct perpetualDLC=new BuildProduct(Product.PERPETUAL,true);
            BuildProduct DancerDLC=new BuildProduct(Product.DANCER,true);
            BuildProduct PriestDLC=new BuildProduct(Product.PRIEST,true);
            BuildProduct GhoulDLC=new BuildProduct(Product.GHOUL,true);
             BuildProduct ExileDLC=new BuildProduct(Product.EXILE,true);

            products.Add(vanillaEdition);
            products.Add(perpetualDLC);
            products.Add(DancerDLC);
            products.Add(PriestDLC);
            products.Add(GhoulDLC);
            products.Add(ExileDLC);

            BuildStorefront buildStorefront=new BuildStorefront(Storefront.Steam,OSs,products);

            MakeDistribution(GameId.CS, buildStorefront);


        }



        [MenuItem("Tools/CS Distribution (Gog)",false, 110)]
        public static void MakeGogDistribution()
        {
            BuildEnvironment env = new BuildEnvironment(GameId.CS, DEFAULT_BUILD_ROOT);

            List<BuildOS> OSs=new List<BuildOS>();
            List<BuildProduct> products=new List<BuildProduct>();

            BuildOS osw=new BuildOS(GameId.CS, BuildTarget.StandaloneWindows64);
            BuildOS oso=new BuildOS(GameId.CS, BuildTarget.StandaloneOSX);
            BuildOS osl=new BuildOS(GameId.CS, BuildTarget.StandaloneLinux64);
            OSs.Add(osw);
            OSs.Add(oso);
            OSs.Add(osl);


            BuildProduct vanillaEdition=new BuildProduct(Product.VANILLA,false);
            BuildProduct perpetualDLC=new BuildProduct(Product.PERPETUAL,true);
            BuildProduct DancerDLC=new BuildProduct(Product.DANCER,true);
            BuildProduct PriestDLC=new BuildProduct(Product.PRIEST,true);
            BuildProduct GhoulDLC=new BuildProduct(Product.GHOUL,true);
            BuildProduct ExileDLC=new BuildProduct(Product.EXILE,true);

            products.Add(vanillaEdition);
            products.Add(perpetualDLC);
            products.Add(DancerDLC);
            products.Add(PriestDLC);
            products.Add(GhoulDLC);
            products.Add(ExileDLC);

            BuildStorefront buildStorefront=new BuildStorefront(Storefront.Gog,OSs,products);

            MakeDistribution(GameId.CS, buildStorefront);
            
        }

        [MenuItem("Tools/CS Distribution (Humble)", false, 110)]
        public static void MakeHumbleDistribution()
        {
            BuildEnvironment env = new BuildEnvironment(GameId.CS, DEFAULT_BUILD_ROOT);

            List<BuildOS> OSs = new List<BuildOS>();
            List<BuildProduct> products = new List<BuildProduct>();

            BuildOS osw = new BuildOS(GameId.CS, BuildTarget.StandaloneWindows64);
            OSs.Add(osw);


            BuildProduct vanillaEdition = new BuildProduct( Product.VANILLA, false);
            BuildProduct perpetualAllDlc = new BuildProduct( Product.PERPETUAL_ALLDLC, false);
            BuildProduct DancerDLC = new BuildProduct( Product.DANCER, true);
            BuildProduct PriestDLC = new BuildProduct( Product.PRIEST, true);
            BuildProduct GhoulDLC = new BuildProduct( Product.GHOUL, true);
            BuildProduct ExileDLC = new BuildProduct( Product.EXILE, true);

            products.Add(vanillaEdition);
            products.Add(perpetualAllDlc);
            products.Add(DancerDLC);
            products.Add(PriestDLC);
            products.Add(GhoulDLC);
            products.Add(ExileDLC);

            BuildStorefront buildStorefront = new BuildStorefront(Storefront.Humble, OSs, products);

            MakeDistribution(GameId.CS, buildStorefront);

        }

        private static void PerformBuild(Product productId, BuildTarget target)
        {
            var product = new BuildProduct(productId, false);

            var os = new BuildOS(product.GetGameId(), target);
            

            var env=new BuildEnvironment(product.GetGameId(), DEFAULT_BUILD_ROOT);
          
            
            

            env.DeleteProductWithOSBuildPath(product,os);

            
        var config=new Config();
            config.SetGame(product.GetGameId()); //gameid might be different from what we've been playing with in the editor. Set it explicitly here to overwrite gameid and contentdir for later build stuff

            if(!Directory.Exists(SCENES_FOLDER))
                throw new ApplicationException($"Can't find scenes folder {SCENES_FOLDER}: exiting");

            List<string> sceneFiles = new List<string>();
            
            sceneFiles.AddRange(Directory.GetFiles(SCENES_FOLDER).ToList().FindAll(f=>f.EndsWith(".unity"))); //the common scene files are just in /assets/scenes

            
            string gameSpecificSceneFilesDir = NoonUtility.JoinPaths(SCENES_FOLDER, product.GetGameId().ToString()); //the game-specific scene files are in /assets/scenes/[gameid]
            sceneFiles.AddRange(Directory.GetFiles(gameSpecificSceneFilesDir).ToList().FindAll(f => f.EndsWith(".unity")));

            foreach (var f in sceneFiles)
            {
                Log("Adding shared scene file to build: " + f);
            }

            string buildPath = env.GetProductWithOSBuildPath(product, os);
            string exePath = NoonUtility.JoinPaths(buildPath, os.ExeName);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                target = target,
                locationPathName = exePath,
                scenes = sceneFiles.ToArray(),

        };
            Log("Building " + target + " version to " + exePath);


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

            PostBuildFileTasks(target, GetParentDirectory(pathToBuiltProject), target);
            
            Log($"----FINISHED BUILDING {target.ToString()} VERSION TO {pathToBuiltProject}");
        }

        private static void PostBuildFileTasks(BuildTarget buildTarget, string pathToBuiltProject,BuildTarget target)
        {
            //get gameid from config file, since we don't have access to all the original spec from the menu command
            //which I *think* is the one  built in /applicationdata - so it's the app name that makes it? some witchcraft. Prolly needs rethinking
            var c = new Config();

            var game = c.GetGame();
            var os = new BuildOS(game, target);

            BuildEnvironment env=new BuildEnvironment(game,DEFAULT_BUILD_ROOT);
            
            string contentFolder=c.GetConfigValueAsString(NoonConstants.CONTENT_FOLDER_NAME_KEY);
            //there's a chance I'll want to come in here and make other changes besides the content folder deletion.

            //Deletes all folders in streamingassets with 'content' in their names, except the one we specifically want to keep.
            //logs the deletion, but still this might cause us angst if we don't keep an eye.
           DeleteContentFoldersExcept(os,pathToBuiltProject, contentFolder);

            //copies ALL libraries: this is the base OS build, so this is as it should be.
            CopyStorefrontLibraries(buildTarget, pathToBuiltProject);
            //gets Application.Version and writes it to version.txt, where it can be used to specify version by upload scripts
            AddVersionNumber(pathToBuiltProject);

            string perpetualEditionsFolderPath =NoonUtility.JoinPaths(env.GetProductWithOSBuildPath(new BuildProduct(Product.PERPETUAL_ALLDLC,false),os));
            BuildPerpetualEdition(pathToBuiltProject,perpetualEditionsFolderPath,os);

            string dlcFolderPath=NoonUtility.JoinPaths(env.GetBaseBuildsPath(), CONST_DLC_FOLDER);

            if(game==GameId.CS) //<--- THIS IS WHY NOTHING IS GOING INTO THE DLC FOLDERS IN 2022, AK
                ExtractDLCFilesFromBaseBuilds(pathToBuiltProject,dlcFolderPath, os);

          //  RunContentTests(buildTarget, builtAtPath);
        }

        private static void DeleteContentFoldersExcept(BuildOS os, string pathToBuiltProject, string nameOfContentFolderToKeep)
        {
  
            string assetsFolderPath = NoonUtility.JoinPaths(pathToBuiltProject, os.GetDataFolderPath(), POST_BUILT_STREAMING_ASSETS_FOLDER);
            Log($"Removing all %content% folders in '{assetsFolderPath}' except '{nameOfContentFolderToKeep}'");
            var contentFolders = Directory.GetDirectories(assetsFolderPath).Where(f=>f.Contains(CONTENT_FOLDER_CONTAINS_STRING));
            
            foreach(var cf in contentFolders)
            {
                DirectoryInfo thisContentDir = new DirectoryInfo(cf);
                if (!string.Equals(thisContentDir.Name,nameOfContentFolderToKeep,StringComparison.OrdinalIgnoreCase))
                {
                    Log($"Deleting {thisContentDir.FullName}");
                    thisContentDir.Delete(true);
                   
                    
                }    
                
            }
        }

        private static void MakeDistribution(GameId game,BuildStorefront storefront)
        {
            BuildEnvironment env = new BuildEnvironment(game,DEFAULT_BUILD_ROOT);

            var distributions = storefront.GetDistributionsForStorefront(env);

            foreach(var d in distributions)
                d.CopyFilesFromEnvironment(env);

            Log($"----------FINISHED {storefront.StoreId} DISTRIBUTION BUILD");
        }

        private static void BuildPerpetualEdition(string from,string to,BuildOS os)
        {
            // Set up the Perpetual Edition, with all its DLC

            Log("Copying whole project with DLC from " + from + " to " + to);

            if (Directory.Exists(to))
                Directory.Delete(to, true);
            NoonUtility.CopyDirectoryRecursively(from, to);


            string semperPath = NoonUtility.JoinPaths(
                to,
                os.GetDataFolderPath(),
                CONST_PERPETUALEDITION_SEMPER_RELATIVE_PATH_TO_FILE);

            WriteSemperFile(semperPath);
        }


        private static void ExtractDLCFilesFromBaseBuilds(string from,string to, BuildOS os)
        {
        // Take the DLCs out of the base edition and into their own directories

   
            foreach (var dlcContentType in ContentTypes)
            foreach (var locale in Locales)
                MoveDlcContent(from, to,os, dlcContentType, locale);
        }



        private static void CopyStorefrontLibraries(BuildTarget target, string builtAtPath)
        {
            Log("Copying Steam libraries to build: " + builtAtPath);
            CopySteamLibraries.Copy(target, builtAtPath);
            Log("Copying Galaxy libraries to build: " + builtAtPath);
            CopyGalaxyLibraries.Copy(target, builtAtPath);
        }

        private static void MoveDlcContent(string from, string to, BuildOS os, string contentOfType, string locale)
        {
            var baseEditionContentPath = NoonUtility.JoinPaths(from,os.GetCoreContentPath(locale),contentOfType);
            Log("Searching for DLC in " + baseEditionContentPath);

            var contentFiles = Directory.GetFiles(baseEditionContentPath).ToList().Where(f => f.EndsWith(".json"));
            string dlcTitleLastLoop =
                "This is a dummy string: whenever it's a new title, we want to create a new directory, but we don't want to recreate it every loop for the same title or we lose previous files of that content type.";
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

                //get the DLC destination location (../DLC/[title]/[platform]/[datafolder]/StreamingAssets/content/core/[contentOfType]/[thatfile]
                string dlcDestinationDir =NoonUtility.JoinPaths(to,dlcTitle, os.GetRelativePath(), os.GetCoreContentPath(locale), contentOfType);

                string dlcFileDestinationPath = NoonUtility.JoinPaths(dlcDestinationDir, dlcFilenameWithoutPath);

                if (dlcTitle != dlcTitleLastLoop)
                    if (Directory.Exists(dlcDestinationDir))
                    {
                        Directory.Delete(dlcDestinationDir, true);
                        Log("Removing old directory: " + dlcDestinationDir);
                    }
                Log("Creating directory: " + dlcDestinationDir);
                Directory.CreateDirectory(dlcDestinationDir);

                Log("Moving file " + contentFilePath + " to " + dlcFileDestinationPath);
                File.Move(contentFilePath, dlcFileDestinationPath);

                dlcTitleLastLoop = dlcTitle;
            }

            // Create the Perpetual Edition DLC
            string semperPath = NoonUtility.JoinPaths(
                to,
                CONST_PERPETUALEDITION_DLCTITLE,
                os.GetRelativePath(),
                os.GetDataFolderPath(),
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
            File.WriteAllText(versionPath, new VersionNumber(Application.version).ToString());
        }








        public static void Log(string message)
        {
            var oldStackTraceLogType = Application.GetStackTraceLogType(LogType.Log);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log("WF>>>>> " + message);
            Application.SetStackTraceLogType(LogType.Log, oldStackTraceLogType);
        }



        private static string GetParentDirectory(string path)
        {
            return Path.GetFullPath(Path.Combine(path, @"../"));
        }

 

    }
}
