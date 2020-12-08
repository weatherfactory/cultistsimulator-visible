using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.TabletopUi;

namespace Assets.Core.Interfaces
{
    public interface IVerb
    {
        string Id { get; }
        void SetId(string id);
        string Label { get; set; }
        string Description { get; set; }
       
        bool Transient { get; }
        string Art { get; }
        Type GetDefaultManifestationType();
        Type GetManifestationType(SphereCategory forSphereCategory);
      SlotSpecification Slot { get; set; }
      List<SlotSpecification> Slots { get; set; }
      bool Startable { get; }
      bool ExclusiveOpen { get; }
      bool CreationAllowedWhenAlreadyExists(Situation s);
      Situation CreateDefaultSituation(TokenLocation anchorLocation);
    }

}
