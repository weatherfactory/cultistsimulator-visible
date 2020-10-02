using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Interfaces
{
    public interface IVerb
    {
        string Id { get; }
        string Label { get; }
        string Description { get; }
        string Species { get; }
        bool Transient { get; }
      SlotSpecification Slot { get; set; }
      List<SlotSpecification> Slots { get; set; }
      bool Startable { get; }
    }

}
