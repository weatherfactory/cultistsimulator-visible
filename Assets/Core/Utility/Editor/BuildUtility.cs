using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public static void OnPostProcessBuilt(BuildTarget target, string exePath)
        {
            string exeFolder = Path.GetDirectoryName(exePath);
            string readmepath=exeFolder+"\\readme.txt";

            Debug.Log(readmepath);

            using (StreamWriter sw = File.CreateText(readmepath))
            {
                sw.Write("THE SCHOLAR'S BUILD - " + target +"\n");
                sw.WriteLine("version: " + NoonUtility.VersionNumber);
            }

        }
    }
}
