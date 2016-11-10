using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Interfaces
{
    public interface IElementCard
    {
        string ElementId { get; }
        int Quantity { get; }
        Dictionary<string, int> GetAspects();
    }
}
