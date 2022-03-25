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
using SecretHistories.Spheres;
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
        public string EntityId { get; set; }
        public int Quantity { get; set; }
        public Dictionary<string,int> Mutations { get; set; }
        public Dictionary<string,string> Illuminations { get; set; }
        public bool Defunct { get; set; }
        public float LifetimeRemaining { get; set; }
        public List<PopulateDominionCommand> Dominions { get; set; } = new List<PopulateDominionCommand>();

        public ElementStackCreationCommand(): this(string.Empty,0)
        {}

        public ElementStackCreationCommand(string elementId, int quantity)
        {
            EntityId = elementId;
            Quantity = quantity;
           Mutations=Element.EmptyMutationsDictionary();
           Illuminations = Element.EmptyIlluminationsDictionary();
        }

        public ITokenPayload Execute(Context context)
        {
            ElementStack elementStackPayload = null;

            try
            {
                var compendium = Watchman.Get<Compendium>();
                  var  element = compendium.GetEntityById<Element>(EntityId);

                  //If we deserialise a stack, we'll already know its ID. If we're creating it for the first time, we need to pick an ID
                  if (String.IsNullOrEmpty(Id))
                     Id = element.DefaultUniqueTokenId();
                  if (LifetimeRemaining == 0 && !Defunct)
                      LifetimeRemaining = element.Lifetime; //I wish I didn't have to use 0 for 'uninitialised' and '0 remaining.'



                var timeshadow = new Timeshadow(element.Lifetime, LifetimeRemaining, element.Resaturate);

                elementStackPayload = new ElementStack(Id,element, Quantity, timeshadow, context);
                
                foreach (var m in Mutations)
                    elementStackPayload.SetMutation(m.Key, m.Value, false);

                foreach(var i in Illuminations)
                    elementStackPayload.SetIllumination(i.Key,i.Value);

                foreach (var d in Dominions)
                    d.Execute(elementStackPayload);

            }
            catch (Exception e)
            {

                NoonUtility.Log("Couldn't create element with ID " + Id + " - " + e.Message + "(This might be an element that no longer exists being referenced in a save file?)");
                elementStackPayload?.Retire(RetirementVFX.None);
            }

     
            return elementStackPayload;
        }


    }
}
