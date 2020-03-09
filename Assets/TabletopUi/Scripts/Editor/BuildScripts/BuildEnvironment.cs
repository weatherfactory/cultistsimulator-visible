using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Utility;
using Noon;

namespace Assets.TabletopUi.Scripts.Editor.BuildScripts
{
    public class BuildEnvironment
    {
        public string BaseBasePath { get; private set; }

        public BuildEnvironment(string basePath)
        {
            BaseBasePath = basePath;
        }

        public string GetProductWithOSBuildPath(BuildProduct p, BuildOS o)
        {
            return NoonUtility.JoinPaths(BaseBasePath, p.GetRelativePath(), o.OSId.ToString());
        }

        public void Log(string message)
        {
            BuildUtility.Log(message);
        }

    }
}
