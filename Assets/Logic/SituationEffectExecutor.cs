using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Noon;
using UnityEngine;

namespace Assets.Logic
{
   public class SituationEffectExecutor
   {

        public void RunEffects(ISituationEffectCommand command, IElementStacksManager stacksManager,IGameEntityStorage storage)
        {
            var aspectsPresent = stacksManager.GetTotalAspects();
            aspectsPresent.CombineAspects(command.Recipe.Aspects);

            RunMutationEffects(command, stacksManager);

            RunXTriggers(stacksManager, aspectsPresent);
            //note: standard effects happen *after* XTrigger effects
            RunDeckEffect(command,stacksManager,storage);
            //and after deck effect
            RunRecipeEffects(command, stacksManager);
            //Do this last: remove any stacks marked for consumption by being placed in a consuming slot
            RunConsumptions(stacksManager);
        }

       private void RunMutationEffects(ISituationEffectCommand command, IElementStacksManager stacksManager)
       {
           foreach(var mutationEffect in command.Recipe.MutationEffects)
           { 
               foreach (var stack in stacksManager.GetStacks())
               {
                    if(stack.GetAspects(true).ContainsKey(mutationEffect.FilterOnAspectId))
                        stack.SetMutation(mutationEffect.MutateAspectId,mutationEffect.MutationLevel);
               }
            }
        }

       private void RunConsumptions(IElementStacksManager stacksManager)
        {

            var stacks = stacksManager.GetStacks();

            for (int i = 0; i < stacks.Count(); i++)
            {
                if (stacks.ElementAt(i) != null && stacks.ElementAt(i).MarkedForConsumption)
                {
                    stacks.ElementAt(i).Retire(true);
                }
            }
        }

        public void RunDeckEffect(ISituationEffectCommand command, IElementStacksManager stacksManager,
            IGameEntityStorage storage)
        {
            var deckIds = command.GetDeckEffects();
            if (deckIds != null && deckIds.Any())
            {
                var dealer = new Dealer(storage);

                foreach (var deckId in deckIds.Keys)
                {
                    var deck = storage.GetDeckInstanceById(deckId);
                    if (deck != null)
                    {
                        for (int i = 1; i <= deckIds[deckId]; i++)
                        {
                            var drawnCardId = dealer.Deal(deck);

                            if (drawnCardId != null)
                            {
                                var source = Source.Fresh(); //ultimately this should correspond to deck
                                stacksManager.ModifyElementQuantity(drawnCardId, 1, source,
                                    new Context(Context.ActionSource.SituationEffect));
                            }
                            else
                            {
                                throw new ApplicationException("Couldn't retrieve a card from deck " + deckId);
                            }
                        }
                    }
                }
            }
        }

        private static void RunRecipeEffects(ISituationEffectCommand command, IElementStacksManager stacksManager)
        {
            foreach (var kvp in command.GetElementChanges())
            {
                var source = Source.Fresh(); //might later be eg Transformed
                stacksManager.ModifyElementQuantity(kvp.Key, kvp.Value, source, new Context(Context.ActionSource.SituationEffect));
            }
        }

        private static void RunXTriggers(IElementStacksManager stacksManager, AspectsDictionary aspectsPresent)
        {
            ICompendium _compendium = Registry.Retrieve<ICompendium>();
            foreach (var eachStack in stacksManager.GetStacks())
            {
                var xTriggers = eachStack.GetXTriggers();
                foreach (var triggerKey in xTriggers.Keys)
                    //for each XTrigger in the stack, check if any of the aspects present in all the recipe's stacks match the trigger key
                    if (aspectsPresent.ContainsKey(triggerKey))
                    {
                        Element effectElement = _compendium.GetElementById(xTriggers[triggerKey]);
                        if (effectElement == null)
                        {
                            NoonUtility.Log("Tried to run an xtrigger with an element effect id that doesn't exist: " + xTriggers[triggerKey],1);
                            return;
                        }

                        if (effectElement.IsAspect)
                        {
                            NoonUtility.Log("Tried to run an xtrigger to transfomr an aspect into another aspect:" + xTriggers[triggerKey] + " - which might ultimately mean a mutationeffect, but doesn't do anything now.", 1);

                        }
                        else
                        {
                         string newElementId = xTriggers[triggerKey];
                            string oldElementId = eachStack.EntityId;
                        int existingQuantity = eachStack.Quantity;
                        //replace the element that has the trigger with the trigger result
                        //eg, if an individual has a Recruiting: individual_b trigger, and there's a Recruiting aspect in the stack, then replace the individual with individual_b
                        //stacksManager.ModifyElementQuantity(eachStack.EntityId, -existingQuantity, Source.Existing(), new Context(Context.ActionSource.SituationEffect));
                        //stacksManager.ModifyElementQuantity(xTriggers[triggerKey], existingQuantity, Source.Existing(), new Context(Context.ActionSource.SituationEffect));

                       eachStack.Populate(newElementId, existingQuantity, Source.Existing());


                        NoonUtility.Log("xtrigger aspect " + triggerKey + " caused " + oldElementId + " to transform into " +
                                        newElementId, 10);

                        }
                    }
            }
        }
    }
}
