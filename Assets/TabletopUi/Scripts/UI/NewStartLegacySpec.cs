using System.Collections.Generic;
using Assets.Core.Entities;
using TabletopUi.Scripts.Services;

namespace TabletopUi.Scripts.UI
{
    public class NewStartLegacySpec
    {
        public string Id { get; }
        public Dictionary<Storefront, string> Links { get; }
        public bool ReleasedByWf;
        public Legacy Legacy;
        public bool IsOfficial;

        public NewStartLegacySpec(string id, Dictionary<Storefront, string> links, bool releasedByWfByWf,Legacy legacy)
        {
            Id = id;
            Links = links;
            ReleasedByWf = releasedByWfByWf;
            Legacy = legacy;
        }
    }
}