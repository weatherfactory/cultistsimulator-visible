using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;

namespace Assets.Core.Commands
{
    /// <summary>
    /// Used when saving/loading games, just to tidy up the code around wrangling these values
    /// </summary>
    public class ElementQuantitySpecification
    {
        public string ElementId { get; private set; }
        public int ElementQuantity { get; private set; }
        public string LocationInfo { get; private set; }
        public int Depth { get; private set; }

        public ElementQuantitySpecification(string elementId, int elementQuantity, string locationInfo)
        {
            ElementId = elementId;
            ElementQuantity = elementQuantity;
            LocationInfo = locationInfo;
            Depth = locationInfo.Count(c => c == SaveConstants.SEPARATOR);
        }
    }
}
