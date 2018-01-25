using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    /// <summary>
    /// Game state info that doesn't change between characters
    /// </summary>
    public class MetaInfo
    {
        public string Version { get; set; }

        public MetaInfo(string version)
        {
            if(version==null)
                throw new ApplicationException("Can't have a null version for MetaInfo");
            
            Version = version;

        }

        public int GetVersionYear()
        {
            int year = Convert.ToInt32(Version.Split('.')[0]);
            return year;
        }

        public int GetVersionMonth()
        {
            int month= Convert.ToInt32(Version.Split('.')[1]);
            return month;
        }

        public char GetVersionMinor()
        {
            char minor = Convert.ToChar(Version.Split('.')[2]);
            return minor;
        }

        public int GetVersionBuildNumber()
        {
            int buildNumber = Convert.ToInt32(Version.Split('.')[3]);
            return buildNumber;
        }
    }
}
