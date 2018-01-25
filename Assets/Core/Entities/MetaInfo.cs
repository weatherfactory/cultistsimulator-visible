using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    public class MetaInfo
    {
        private VersionNumber _versionNumber;

        public MetaInfo(VersionNumber versionNumber)
        {
            _versionNumber = versionNumber;
        }

        public VersionNumber VersionNumber
        {
            get { return _versionNumber; }
        }
    }
}
