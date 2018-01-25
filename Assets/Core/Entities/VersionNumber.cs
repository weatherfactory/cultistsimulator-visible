using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    /// <summary>
    /// Game state info that doesn't change between characters
    /// </summary>
    public class VersionNumber
    {
        private readonly string _version;

        public string Version
        {
            get { return _version; }
        }

        public override string ToString()
        {
            return _version;
        }

        public VersionNumber(string version)
        {
            if(version==null)
                throw new ApplicationException("Can't have a null version for VersionNumber");
            
            _version = version;

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

        public bool MajorVersionMatches(string matchingversion)
        {
            var versionToMatch=new VersionNumber(matchingversion);
            return(GetVersionYear() == versionToMatch.GetVersionYear() 
                       && GetVersionMonth() ==versionToMatch.GetVersionMonth());
                   }
    }
}
