using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Enums;

namespace SecretHistories.Entities
{
    public class MetaInfo
    {
        public VersionNumber VersionNumber { get; }
        public Storefront Storefront { get; }

        public MetaInfo(VersionNumber versionNumber, Storefront storefront)
        {
            VersionNumber = versionNumber;
            Storefront = storefront;
        }


    }
}
