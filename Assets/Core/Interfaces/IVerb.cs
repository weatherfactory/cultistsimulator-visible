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
        bool Transient { get; }
      SlotSpecification Slot { get; set; }
    }

}
