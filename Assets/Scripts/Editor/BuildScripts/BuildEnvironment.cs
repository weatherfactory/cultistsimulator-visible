using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Utility;
using JetBrains.Annotations;
using Noon;
using UnityEditor;

namespace Assets.TabletopUi.Scripts.Editor.BuildScripts
{
    public class BuildEnvironment
    {
        public string BuildRoot { get; private set; }
        private const string BASE_OS_BUILDS = "BASE_OS_BUILDS";

        public BuildEnvironment(string root)
        {
            BuildRoot = root;
        }

        public string GetBaseBuildsPath()
        {
            return NoonUtility.JoinPaths(BuildRoot, BASE_OS_BUILDS);
        }

        public string GetProductWithOSBuildPath(BuildProduct p, BuildOS o)
        {
            return NoonUtility.JoinPaths(GetBaseBuildsPath(), p.GetRelativePath(), o.OSId.ToString());
        }

        public void Log(string message)
        {
            NoonUtility.Log(message);
        }

        public void LogError(string message)
        {
            NoonUtility.Log(message,2);
        }

        public void DeleteProductWithOSBuildPath(BuildProduct p, BuildOS o)
        {
            // Clear the build directory of any of the intermediate results of a previous build
            DirectoryInfo ProductOSDir = new DirectoryInfo(GetProductWithOSBuildPath(p,o));

            if(ProductOSDir.Exists)
            {
                foreach (var file in ProductOSDir.GetFiles())
                    File.Delete(file.FullName);
                foreach (var directory in ProductOSDir.GetDirectories())
                    Directory.Delete(directory.FullName, true);
            }
        }

    }
}
