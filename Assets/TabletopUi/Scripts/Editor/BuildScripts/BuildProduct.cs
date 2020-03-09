using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noon;

namespace Assets.TabletopUi.Scripts.Editor.BuildScripts
{
    public class Product
    {
        public string Id { get; private set; }
        public bool IsDLC { get; private set; }
        private BuildEnvironment _fromEnvironment;


        public Product(BuildEnvironment fromEnvironment, string id, bool isDLC)
        {
            _fromEnvironment = fromEnvironment;
            Id = id;
            IsDLC = isDLC;
        }

        public string GetRelativePath()
        {
            if (IsDLC)
                return NoonUtility.JoinPaths("DLC\\", Id);
            else
                return Id;
        }

        public string BuiltAtPath()
        {
            return NoonUtility.JoinPaths(_fromEnvironment.BasePath, GetRelativePath());

        }
    }
}
