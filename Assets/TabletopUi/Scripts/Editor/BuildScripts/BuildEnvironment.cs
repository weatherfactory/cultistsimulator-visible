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
        public string BasePath { get; private set; }

        public BuildEnvironment(string path)
        {
            BasePath = path;
        }

        public string GetProductWithOSBuildPath(Product p, OS o)
        {
            return NoonUtility.JoinPaths(BasePath, p.GetRelativeBuildPath(), o.OSId.ToString());
        }

        public void Log(string message)
        {
            BuildUtility.Log(message);
        }

    }
}
