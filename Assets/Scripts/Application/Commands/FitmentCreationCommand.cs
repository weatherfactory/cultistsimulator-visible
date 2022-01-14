using System;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Abstract;
using SecretHistories.Commands.SituationCommands;
using UnityEngine;

namespace SecretHistories.Commands
{
    public class FitmentCreationCommand : ITokenPayloadCreationCommand,IEncaustment
    {

 
        public ITokenPayload Execute(Context context)
        {
            throw new NotImplementedException();
            //ElementStack elementStack = null;

            //try
            //{
            //    var compendium = Watchman.Get<Compendium>();
            //    var element = compendium.GetEntityById<Element>(EntityId);

            //    //If we deserialise a stack, we'll already know its ID. If we're creating it for the first time, we need to pick an ID
            //    if (String.IsNullOrEmpty(Id))
            //        Id = element.DefaultUniqueTokenId();
            //    if (LifetimeRemaining == 0 && !Defunct)
            //        LifetimeRemaining = element.Lifetime; //I wish I didn't have to use 0 for 'uninitialised' and '0 remaining.'



            //    var timeshadow = new Timeshadow(element.Lifetime, LifetimeRemaining, element.Resaturate);

            //    elementStack = new ElementStack(Id, element, Quantity, timeshadow, context);

            //    foreach (var m in Mutations)
            //        elementStack.SetMutation(m.Key, m.Value, false);

            //    foreach (var i in Illuminations)
            //        elementStack.SetIllumination(i.Key, i.Value);

            //}
            //catch (Exception e)
            //{

            //    NoonUtility.Log("Couldn't create element with ID " + Id + " - " + e.Message + "(This might be an element that no longer exists being referenced in a save file?)");
            //    elementStack?.Retire(RetirementVFX.None);
            //}


            //return elementStack;

        }

        public int Quantity { get; }
        public List<PopulateDominionCommand> Dominions { get; set; }
    }
}