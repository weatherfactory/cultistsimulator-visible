using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.NullObjects;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Constants.Events
{
    public class SphereContentsChangedEventArgs
    {
        public Sphere Sphere { get; set; }
        public Token TokenAdded { get; set; }
        public Token TokenRemoved { get; set; }
        public Context Context { get; set; }

    }
}  
 
