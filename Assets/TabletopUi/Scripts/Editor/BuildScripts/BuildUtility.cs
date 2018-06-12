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

        [PostProcessBuild]
        public static void PostProcessHook(BuildTarget target, string pathToBuiltProject)
        {
            CopySteamLibraries.Copy(target, pathToBuiltProject);
            CopyGalaxyLibraries.Copy(target, pathToBuiltProject);
            Thread.Sleep(1000);
            string exeFolder = Path.GetDirectoryName(pathToBuiltProject);
            AddVersionNumber(exeFolder);
            RenameExecutable(target, pathToBuiltProject, exeFolder);


        }

        public static void AddVersionNumber(string exeFolder)
        {
            
            string versionPath=exeFolder+"/v" + NoonUtility.VersionNumber + ".txt";

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

