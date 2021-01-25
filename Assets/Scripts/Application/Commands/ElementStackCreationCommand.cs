using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Abstract;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Entities;
using UnityEngine;

namespace SecretHistories.Commands
{
    
    public class ElementStackCreationCommand
    {
        /// <summary>
        /// The element id
        /// </summary>
        public string Id { get; set; }
        public int Quantity { get; set; }
        public Dictionary<string,int> Mutations { get; set; }
        public IlluminateLibrarian IlluminateLibrarian { get; set; }

        public float LifetimeRemaining { get; set; }
        public bool MarkedForConsumption { get; set; }
        
        public ElementStackCreationCommand()
        {}
    }
}
