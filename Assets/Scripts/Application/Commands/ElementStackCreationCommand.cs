using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Abstract;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SecretHistories.Commands
{
    
    public class ElementStackCreationCommand
    {
        /// <summary>
        /// The element id
        /// </summary>
        public string PayloadId { get; set; }
        public int Quantity { get; set; }
        public Dictionary<string,int> Mutations { get; set; }
        public IlluminateLibrarian IlluminateLibrarian { get; set; }
        public bool Defunct { get; set; }

        public float LifetimeRemaining { get; set; }

        private Element element;

        public ElementStackCreationCommand(): this(string.Empty,0)
        {}

        public ElementStackCreationCommand(string elementPayloadId, int quantity)
        {
            PayloadId = elementPayloadId;
            Quantity = quantity;
           element = Watchman.Get<Compendium>().GetEntityById<Element>(PayloadId) ?? new NullElement();
           LifetimeRemaining = element.Lifetime; //set the element lifetime as the default lifetimeremaining for the stack; we can override this subsequently before executing if we like
           IlluminateLibrarian=new IlluminateLibrarian();
           Mutations=Element.EmptyMutationsDictionary();
        }

        public ElementStack Execute(Context context)
        {
            ElementStack elementStack = null;

            try
            {
                elementStack = new ElementStack(element, Quantity, LifetimeRemaining, context);
                foreach (var m in Mutations)
                    elementStack.SetMutation(m.Key, m.Value, false);

                elementStack.IlluminateLibrarian = IlluminateLibrarian; }
            catch (Exception e)
            {

                NoonUtility.Log("Couldn't create element with ID " + PayloadId + " - " + e.Message + "(This might be an element that no longer exists being referenced in a save file?)");
                elementStack?.Retire(RetirementVFX.None);
            }

            
            return elementStack;
        }
    }
}
