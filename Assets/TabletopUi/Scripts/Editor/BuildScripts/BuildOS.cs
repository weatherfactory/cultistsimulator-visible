using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.TabletopUi.Scripts.Editor.BuildScripts
{
    public class BuildOS
    {
        public OSId OSId { get; private set; }


        public BuildOS(OSId id)
        {
            OSId = id;

        }


        public string GetRelativePath()
        {
            return OSId.ToString();
        }


        public string GetStreamingAssetsLocation()
        {
            //switch based on OSId
            throw new NotImplementedException();
        }
        
    }

    public enum OSId
    {
        Windows,
        OSX,
        Linux
    }
}
