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
                "Assets/TabletopUi/Global.unity",
            };

            return scenes;
        }

        private static void MoveDLCContent()
        {
            //for every folder in [outputpath]/[datafolder]/StreamingAssets/content/core/
            //for every file in that folder
            //does it begin DLC_
            //if so, get its title (DLC_[title]_)
            //get the DLC location (../DLC/[DLCName]/[platform]/[datafolder]/StreamingAssets/content/core/[currentfolder]/[thatfile]
            //throw an error if it doesn't exist
            //move (don't copy) the file to that location.

        }

        public static void AddVersionNumber(string exeFolder)
        {

            string versionPath = exeFolder + "/version.txt";

            File.WriteAllText(versionPath, NoonUtility.VersionNumber.ToString());


        }

        private static void PostBuildFileTasks(BuildTarget target, string pathToBuiltProject)
        {
            CopySteamLibraries.Copy(target, pathToBuiltProject);
            CopyGalaxyLibraries.Copy(target, pathToBuiltProject);
            string exeFolder = Path.GetDirectoryName(pathToBuiltProject);
            AddVersionNumber(exeFolder);
        }

    private static string GetBuildPath()
        {
            return System.Environment.GetCommandLineArgs()[1];
        }

        public static void PerformWindowsBuild()
        {
            try
            {


            BuildPlayerOptions windowsBuildPlayerOptions =
                new BuildPlayerOptions
                {
                    target = BuildTarget.StandaloneWindows,
                    locationPathName = GetBuildPath() + "cultistsimulator.exe"
                };

            Debug.Log(">>>>>> Building Windows version to " + GetBuildPath());

            windowsBuildPlayerOptions.scenes = GetScenes();

            BuildPipeline.BuildPlayer(windowsBuildPlayerOptions);
            PostBuildFileTasks(windowsBuildPlayerOptions.target, GetBuildPath());
            }
            catch (Exception e)
            {
                Debug.Log("ERROR: " + e.Message);
            }

        }

        public static void PerformOsxBuild()
        {
            try
            {


            BuildPlayerOptions osxBuildPlayerOptions = new BuildPlayerOptions
            {
                target = BuildTarget.StandaloneOSX,
                locationPathName = GetBuildPath() + "OSX.app",
                scenes = GetScenes()
            };

            Debug.Log(">>>>>> Building OSX version to " + GetBuildPath());

            BuildPipeline.BuildPlayer(osxBuildPlayerOptions);

            //for some reason, the OSX build barfs when copying the steam libraries in PostProcessHook...but not in here.
            //So I've moved it to here.
            //does the folder not exist at that point?

        PostBuildFileTasks(osxBuildPlayerOptions.target,GetBuildPath());

            }
            catch (Exception e)
            {
                Debug.Log("ERROR: " + e.Message);
            }
        }

        public static void PerformLinuxBuild()
        {
            try
            {


            BuildPlayerOptions linuxBuildPlayerOptions =
                new BuildPlayerOptions
                {
                    target = BuildTarget.StandaloneLinuxUniversal,
                    locationPathName =  GetBuildPath()+ "CS.x86",
                    scenes = GetScenes()
                };
            Debug.Log(">>>>>> Building Linux version to " + GetBuildPath());

            BuildPipeline.BuildPlayer(linuxBuildPlayerOptions);

            PostBuildFileTasks(linuxBuildPlayerOptions.target, GetBuildPath());
            }
            catch (Exception e)
            {
                Debug.Log("ERROR: " + e.Message);
            }
        }



        //[PostProcessBuild]
        //public static void PostProcessHook(BuildTarget target, string pathToBuiltProject)
        //{
        //    try
        //    {

        //    CopySteamLibraries.Copy(target, pathToBuiltProject);
        //    CopyGalaxyLibraries.Copy(target, pathToBuiltProject);
            
        //    string exeFolder = Path.GetDirectoryName(pathToBuiltProject);
        //    AddVersionNumber(exeFolder);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.Log("ERROR: " + e.Message);
        //    }


        //}



        //public static void RenameExecutable(BuildTarget target, string pathToBuiltProject,string exeFolder)
        //{
        //    if (target == BuildTarget.StandaloneOSX)
        //        return; //we dont rename OSX and their weird package file folder thing

        //        string currentExeName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
        //    string renameExecutableTo;
        //    string dataFolder;
        //    string renameDataFolderTo;
        //    if (target == BuildTarget.StandaloneLinux || target == BuildTarget.StandaloneLinux64)
        //    {
        //        //I just wrote the code below, but now it looks like Unity still lets us decide the Linux executable name.
        //        return;
                
        //        //renameExecutableTo = exeFolder + "/CS.x86";
        //        //dataFolder = exeFolder + "/" + currentExeName + "_Data";
        //        //renameDataFolderTo = exeFolder + "/CS_Data";
        //    }

        //    else if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
        //    {

        //        renameExecutableTo = exeFolder + "/cultistsimulator.exe";
        //        dataFolder = exeFolder + "/" + currentExeName + "_Data";
        //        renameDataFolderTo = exeFolder + "/cultistsimulator_Data";

        //    }

        //    else
        //    {
        //        throw new ApplicationException("Can't find a way to handle this build target in RenameExecutable: " +
        //                                       target);
        //    }

        //    if (File.Exists(renameExecutableTo))
        //            File.Delete(renameExecutableTo);
        //        if(Directory.Exists(renameDataFolderTo))
        //            Directory.Delete(renameDataFolderTo,true);

        //   File.Move(pathToBuiltProject, renameExecutableTo);
        //    Directory.Move(dataFolder,renameDataFolderTo);
        //    }
        }
    }

