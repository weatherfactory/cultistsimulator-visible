using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Abstract;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;

using UnityEngine;

namespace SecretHistories.Commands
{
    
    public class ElementStackCreationCommand
    {
        public string ElementId { get; set; }
        public int ElementQuantity { get; set; }
        public string LocationInfo { get; set; }
        public Dictionary<string,int> Mutations { get; set; }
        public Dictionary<string, string> Illuminations { get; set; }

        public int Depth { get; set; }
        public float LifetimeRemaining { get; set; }
        public bool MarkedForConsumption { get; set; }
		public Context Context { get; set; }

        public ElementStackCreationCommand(string elementId, int elementQuantity, string locationInfo,Dictionary<string,int> mutations,Dictionary<string,string> illuminations, float lifetimeRemaining,bool markedForConsumption,Context context)
        {
            ElementId = elementId;
            ElementQuantity = elementQuantity;
            LocationInfo = locationInfo;
            Depth = locationInfo.Count(c => c == SpherePath.SEPARATOR);
            Mutations=new Dictionary<string, int>(mutations);
            Illuminations=new Dictionary<string, string>(illuminations);
            LifetimeRemaining = lifetimeRemaining;
            MarkedForConsumption = markedForConsumption;
			Context= context;
        }

        public ElementStackCreationCommand()
        {}
    }
}
