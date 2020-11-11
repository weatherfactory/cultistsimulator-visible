using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using UnityEngine;

namespace Assets.Core.Commands
{
    /// <summary>
    /// Used when saving/loading games, just to tidy up the code around wrangling these values
    /// </summary>
    public class StackCreationCommand
    {
        public string ElementId { get; private set; }
        public int ElementQuantity { get; private set; }
        public string LocationInfo { get; private set; }
        public Dictionary<string,int> Mutations { get; private set; }
        public Dictionary<string, string> Illuminations { get; private set; }

        public int Depth { get; private set; }
        public float LifetimeRemaining { get; private set; }
        public bool MarkedForConsumption { get; private set; }
		public TokenLocation Location { get; private set; }

        public StackCreationCommand(string elementId, int elementQuantity, string locationInfo,Dictionary<string,int> mutations,Dictionary<string,string> illuminations, float lifetimeRemaining,bool markedForConsumption,TokenLocation location)
        {
            ElementId = elementId;
            ElementQuantity = elementQuantity;
            LocationInfo = locationInfo;
            Depth = locationInfo.Count(c => c == SpherePath.SEPARATOR);
            Mutations=new Dictionary<string, int>(mutations);
            Illuminations=new Dictionary<string, string>(illuminations);
            LifetimeRemaining = lifetimeRemaining;
            MarkedForConsumption = markedForConsumption;
			Location= location;
        }
    }
}
