using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Noon;

namespace Assets.TabletopUi.Scripts.Editor.BuildScripts
{
    public class BuildProduct
    {
        public Product Id { get; private set; }
        public bool IsDLC { get; private set; }
        private BuildEnvironment _fromEnvironment;


        public BuildProduct(BuildEnvironment fromEnvironment, Product id, bool isDLC)
        {
            _fromEnvironment = fromEnvironment;
            Id = id;
            IsDLC = isDLC;
        }

        public string GetRelativePath()
        {
            if (IsDLC)
                return NoonUtility.JoinPaths("DLC\\", Id.ToString());
            else
                return Id.ToString();
        }


    }


    public enum Product
    {
        VANILLA=1,
        PERPETUAL_ALLDLC=2,
        PERPETUAL=3,
        DANCER=4,
        PRIEST=5,
        GHOUL=6,
        EXILE=7

    }
}
