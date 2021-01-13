using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;

namespace SecretHistories.Interfaces
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
      SphereSpec Slot { get; set; }
      List<SphereSpec> Slots { get; set; }
      bool Startable { get; }
      bool ExclusiveOpen { get; }
      Situation CreateDefaultSituation(TokenLocation anchorLocation);
    }

}
