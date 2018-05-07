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
    public class ElementStackSpecification
    {
        public string ElementId { get; private set; }
        public int ElementQuantity { get; private set; }
        public string LocationInfo { get; private set; }
        public Dictionary<string,int> Mutations { get; private set; }
        public int Depth { get; private set; }
        public float LifetimeRemaining { get; private set; }

        public ElementStackSpecification(string elementId, int elementQuantity, string locationInfo,Dictionary<string,int> mutations,float lifetimeRemaining)
        {
            ElementId = elementId;
            ElementQuantity = elementQuantity;
            LocationInfo = locationInfo;
            Depth = locationInfo.Count(c => c == SaveConstants.SEPARATOR);
            Mutations=new Dictionary<string, int>(mutations);
            LifetimeRemaining = lifetimeRemaining;
        }
    }
}
