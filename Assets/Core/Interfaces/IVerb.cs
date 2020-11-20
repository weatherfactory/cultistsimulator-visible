using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;

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
        Type AnchorManifestationType { get; }
      SlotSpecification Slot { get; set; }
      List<SlotSpecification> Slots { get; set; }
      bool Startable { get; }
      bool ExclusiveOpen { get; }
      bool CreationAllowedWhenAlreadyExists(Situation s);
    }

}
