using System.Collections.Generic;
using Assets.Core.Entities;
using TabletopUi.Scripts.Services;

namespace TabletopUi.Scripts.UI
{
    public class StartableLegacySpec
    {
        public string Id { get; }
        public string LocLabelIfNotInstalled { get; }
        public Dictionary<Storefront, string> Links { get; }
        public bool ReleasedByWf;
        public Legacy Legacy;
        public bool IsOfficial;

        public StartableLegacySpec(string id, string locLabelIfNotInstalled, Dictionary<Storefront, string> links, bool releasedByWfByWf,Legacy legacy)
        {
            Id = id;
            LocLabelIfNotInstalled = locLabelIfNotInstalled;
            Links = links;
            ReleasedByWf = releasedByWfByWf;
            Legacy = legacy;
        }
    }
}