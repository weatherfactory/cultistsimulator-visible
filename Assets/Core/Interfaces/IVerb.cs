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
        string Label { get; }
        string Description { get; }
        string SpeciesId { get; }
        Species Species { get; }
        bool Transient { get; }
        string Art { get; }
      SlotSpecification Slot { get; set; }
      List<SlotSpecification> Slots { get; set; }
      bool Startable { get; }
      bool AllowMultipleInstances { get; }
    }

}
