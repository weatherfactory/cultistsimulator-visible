using System.Collections.Generic;
using Assets.Core.Entities;
using TabletopUi.Scripts.Services;

namespace TabletopUi.Scripts.UI
{
    public class NewStartLegacySpec
    {
        public string Id { get; }
        public Dictionary<Storefront, string> Links { get; }
        public bool Released;
        public Legacy Legacy;

        public NewStartLegacySpec(string id, Dictionary<Storefront, string> links, bool released,Legacy legacy)
        {
            Id = id;
            Links = links;
            Released = released;
            Legacy = legacy;
        }
    }
}