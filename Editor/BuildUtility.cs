using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OrbCreationExtensions;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Assets.Core.Utility
{
    public class BuildUtility
    {
        public static string VersionNumber = "2017.12.b.3";

        [PostProcessBuild(1)]
        public static void OnPostProcessBuilt(BuildTarget target,string path)
        {
            using (StreamWriter sw = File.CreateText(path + "readme.txt"))
            {
                sw.Write("THE SCHOLAR'S BUILD - " + target);
                sw.WriteLine("version:" + VersionNumber);
            }                
        }
    }
}
