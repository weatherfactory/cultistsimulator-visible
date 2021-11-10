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
        public GameId GameId { get; }

        public MetaInfo(GameId gameId,VersionNumber versionNumber, Storefront storefront)
        {
            GameId = gameId;
            VersionNumber = versionNumber;
            Storefront = storefront;
        }


    }
}
