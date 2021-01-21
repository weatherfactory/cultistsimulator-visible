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
        Type GetManifestationType(SphereCategory forSphereCategory);
      List<SphereSpec> Thresholds { get; }
      bool Startable { get; }
      bool ExclusiveOpen { get; }
    }

}
