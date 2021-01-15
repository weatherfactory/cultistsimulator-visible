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
using UnityEngine.EventSystems;

namespace SecretHistories.Constants.Events
{
    public class SphereContentsChangedEventArgs
    {
        public Sphere Sphere { get; set; }
        public Token TokenAdded { get; set; }
        public Token TokenRemoved { get; set; }
        public Context Context { get; set; }

        public SphereContentsChangedEventArgs()
        {
            TokenAdded=new NullToken();
            TokenRemoved=new NullToken();
        }
        //this could use another property to indicate element quantity changes without token changes
        //but then of course we have three independent properties to check...
    }
}  
 
