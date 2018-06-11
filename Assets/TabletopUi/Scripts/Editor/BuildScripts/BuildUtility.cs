using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Facepunch.Editor;
using Galaxy;
using Noon;
using OrbCreationExtensions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Assets.Core.Utility
{
    public class BuildUtility
    {

        [PostProcessBuild]
        public static void PostProcessHook(BuildTarget target, string pathToBuiltProject)
        {
            AddVersionNumber(pathToBuiltProject);
            CopySteamLibraries.Copy(target,pathToBuiltProject);
            CopyGalaxyLibraries.Copy(target, pathToBuiltProject);
            RenameExecutable(pathToBuiltProject);
        }

        public static void AddVersionNumber(string pathToBuiltProject)
        {
            string exeFolder = Path.GetDirectoryName(pathToBuiltProject);
            string versionPath=exeFolder+"/v" + NoonUtility.VersionNumber + ".txt";

            File.WriteAllText(versionPath,NoonUtility.VersionNumber.ToString());


        }

        public static void RenameExecutable(string pathToBuildProject)
        {

        }
    }
}
