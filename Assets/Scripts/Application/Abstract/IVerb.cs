using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;

namespace SecretHistories.Interfaces
{
    public interface IVerb
    {
        void SetId(string id);
        string Id { get; }
        string Label { get; set; }
        string Description { get; set; }
       
        bool Transient { get; }
        string Art { get; }
        List<SphereSpec> Thresholds { get; }
      bool ExclusiveOpen { get; }
    }

}
