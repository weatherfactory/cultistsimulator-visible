using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Application.Abstract;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Logic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SecretHistories.Commands
{
    
    public class ElementStackCreationCommand: ITokenPayloadCreationCommand,IEncaustment
    {
        /// <summary>
        /// The element id
        /// </summary>
        public string Id { get; set; }
        public int Quantity { get; set; }
        public Dictionary<string,int> Mutations { get; set; }
        public Dictionary<string,string> Illuminations { get; set; }
        public bool Defunct { get; set; }
        public FucinePath CachedParentPath { get; set; }
        public float LifetimeRemaining { get; set; }
        public List<PopulateDominionCommand> Dominions { get; set; } = new List<PopulateDominionCommand>();

        public ElementStackCreationCommand(): this(string.Empty,0)
        {}

        public ElementStackCreationCommand(string elementId, int quantity)
        {
            Id = elementId;
            Quantity = quantity;
           Mutations=Element.EmptyMutationsDictionary();
           Illuminations = Element.EmptyIlluminationsDictionary();
        }

        public ITokenPayload Execute(Context context,FucinePath atSpherePath)
        {
            ElementStack elementStack = null;


            try
            {
                var compendium = Watchman.Get<Compendium>();
                  var  element = compendium.GetEntityById<Element>(Id);

                  var timeshadow = new Timeshadow(element.Lifetime, LifetimeRemaining, element.Resaturate);

                elementStack = new ElementStack(element, Quantity, timeshadow, context);
                foreach (var m in Mutations)
                    elementStack.SetMutation(m.Key, m.Value, false);

                foreach(var i in Illuminations)
                    elementStack.SetIllumination(i.Key,i.Value);

                elementStack.SetParentPath(atSpherePath);
            }
            catch (Exception e)
            {

                NoonUtility.Log("Couldn't create element with ID " + Id + " - " + e.Message + "(This might be an element that no longer exists being referenced in a save file?)");
                elementStack?.Retire(RetirementVFX.None);
            }

     
            return elementStack;
        }


    }
}
