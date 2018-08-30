using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Facepunch.Editor;
using Galaxy;
using Noon;
using OrbCreationExtensions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.WSA;

namespace Assets.Core.Utility
{
    public class BuildUtility
    {
        
        private static string[] GetScenes()
        {
            string[] scenes =
            {
                "Assets/TabletopUi/Logo.unity",
                "Assets/TabletopUi/Quote.unity",
                "Assets/TabletopUi/Menu.unity",
                "Assets/TabletopUi/Tabletop.unity",
                "Assets/TabletopUi/GameOver.unity",
                "Assets/TabletopUi/NewGame.unity",
            };

            return scenes;
        }

        public static void PerformWindowsBuild()
        {
            BuildPlayerOptions windowsBuildPlayerOptions=new BuildPlayerOptions();

            windowsBuildPlayerOptions.target = BuildTarget.StandaloneWindows;
            windowsBuildPlayerOptions.locationPathName = "D:/Dropbox (Weather Factory)/CS/builds/Windows/cultistsimulator.exe";
            windowsBuildPlayerOptions.scenes = GetScenes();

            BuildPipeline.BuildPlayer(windowsBuildPlayerOptions);
        }

        public static void PerformOsxBuild()
        {
            BuildPlayerOptions osxBuildPlayerOptions = new BuildPlayerOptions();

            osxBuildPlayerOptions.target = BuildTarget.StandaloneOSX;
            osxBuildPlayerOptions.locationPathName = "D:/Dropbox (Weather Factory)/CS/builds/OSX/OSX.app";
            osxBuildPlayerOptions.scenes = GetScenes();

            BuildPipeline.BuildPlayer(osxBuildPlayerOptions);

            //for some reason, the OSX build barfs when copying the steam libraries in PostProcessHook...but not in here.
            //does the folder not exist at that point?
            //I am doing this painful hardcoded thing for now, repeating the postprocesshook for OSX only.

            string pathToBuiltProject = "D:/Dropbox (Weather Factory)/CS/builds/OSX/";
            CopySteamLibraries.Copy(osxBuildPlayerOptions.target, "D:/Dropbox (Weather Factory)/CS/builds/OSX/");
            CopyGalaxyLibraries.Copy(osxBuildPlayerOptions.target, "D:/Dropbox (Weather Factory)/CS/builds/OSX/");
            string exeFolder = Path.GetDirectoryName(pathToBuiltProject);
            AddVersionNumber(exeFolder);

        }

        public static void PerformLinuxBuild()
        {
            BuildPlayerOptions linuxBuildPlayerOptions = new BuildPlayerOptions();
            linuxBuildPlayerOptions.target = BuildTarget.StandaloneLinuxUniversal;
            linuxBuildPlayerOptions.locationPathName = "D:/Dropbox (Weather Factory)/CS/builds/Linux/CS.x86";
            linuxBuildPlayerOptions.scenes = GetScenes();

            BuildPipeline.BuildPlayer(linuxBuildPlayerOptions);
        }



        [PostProcessBuild]
        public static void PostProcessHook(BuildTarget target, string pathToBuiltProject)
        {
            try
            {

            CopySteamLibraries.Copy(target, pathToBuiltProject);
            CopyGalaxyLibraries.Copy(target, pathToBuiltProject);
            
            string exeFolder = Path.GetDirectoryName(pathToBuiltProject);
            AddVersionNumber(exeFolder);
            }
            catch (Exception e)
            {
                Debug.Log("ERROR:" + e.Message);
            }


        }

        public static void AddVersionNumber(string exeFolder)
        {
            
            string versionPath=exeFolder+"/version.txt";

            File.WriteAllText(versionPath,NoonUtility.VersionNumber.ToString());


        }

        public static void RenameExecutable(BuildTarget target, string pathToBuiltProject,string exeFolder)
        {
            if (target == BuildTarget.StandaloneOSX)
                return; //we dont rename OSX and their weird package file folder thing

                string currentExeName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
            string renameExecutableTo;
            string dataFolder;
            string renameDataFolderTo;
            if (target == BuildTarget.StandaloneLinux || target == BuildTarget.StandaloneLinux64)
            {
                //I just wrote the code below, but now it looks like Unity still lets us decide the Linux executable name.
                return;
                
                //renameExecutableTo = exeFolder + "/CS.x86";
                //dataFolder = exeFolder + "/" + currentExeName + "_Data";
                //renameDataFolderTo = exeFolder + "/CS_Data";
            }

            else if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            {

                renameExecutableTo = exeFolder + "/cultistsimulator.exe";
                dataFolder = exeFolder + "/" + currentExeName + "_Data";
                renameDataFolderTo = exeFolder + "/cultistsimulator_Data";

            }

            else
            {
                throw new ApplicationException("Can't find a way to handle this build target in RenameExecutable: " +
                                               target);
            }

            if (File.Exists(renameExecutableTo))
                    File.Delete(renameExecutableTo);
                if(Directory.Exists(renameDataFolderTo))
                    Directory.Delete(renameDataFolderTo,true);

           File.Move(pathToBuiltProject, renameExecutableTo);
            Directory.Move(dataFolder,renameDataFolderTo);
            }
        }
    }

