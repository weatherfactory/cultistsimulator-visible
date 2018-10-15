﻿using System;
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
            var recipeAspects = command.Recipe.Aspects;
            var cardAspects = stacksManager.GetTotalAspects();
 

            RunMutationEffects(command, stacksManager);

            //note: xtriggers for recipe aspects happen before xtriggers for card aspects. Within that precedence, aspects take effect in non-specific order.
            //I think this will generally make sense. Recipe aspects are 'specifically, I want to do this'
            //Card aspects are 'this should generally happen'
            //If this basic logic doesn't work, solutions under consideration: (1) xtrigger priorities (2) feeding a stack back in if it's transformed to react to its new xtriggers (with guard against loop)

            RunXTriggers(stacksManager, recipeAspects);
            RunXTriggers(stacksManager, cardAspects);

            //note: standard effects happen *after* XTrigger effects
            RunDeckEffect(command,stacksManager,storage);
            //and after deck effect
            RunRecipeEffects(command, stacksManager);
            //Do this last: remove any stacks marked for consumption by being placed in a consuming slot
            RunConsumptions(stacksManager); //NOTE: If a stack has just been transformed into another element, all sins are forgiven. It won't be consumed.
        }

       private void RunMutationEffects(ISituationEffectCommand command, IElementStacksManager stacksManager)
       {
           foreach(var mutationEffect in command.Recipe.MutationEffects)
           { 
               foreach (var stack in stacksManager.GetStacks())
               {
                    if(stack.GetAspects(true).ContainsKey(mutationEffect.FilterOnAspectId))
                        stack.SetMutation(mutationEffect.MutateAspectId,mutationEffect.MutationLevel,mutationEffect.Additive);
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

               foreach (var eachStackMutation in eachStack.GetCurrentMutations())
               {
                   //first we apply xtriggers to mutations - but not to the default stack aspects.
                   //we do this first in the expectation that the stack will generally get repopulated next, if there's a relevant xtrigger
                   var stackMutationBaseAspect = _compendium.GetElementById(eachStackMutation.Key);
                   if (stackMutationBaseAspect == null)
                   {
                       NoonUtility.Log("Mutation aspect id doesn't exist: " + eachStackMutation.Key);
                   }
                   else
                   {
                       foreach (var mutationXTrigger in stackMutationBaseAspect.XTriggers)
                       {
                           if (aspectsPresent.ContainsKey(mutationXTrigger.Key))
                           {
                               string newMutationId = mutationXTrigger.Value;
                               string oldMutationId = eachStackMutation.Key;
                               int existingLevel = eachStackMutation.Value;
                               eachStack.SetMutation(oldMutationId, 0, false);
                               eachStack.SetMutation(newMutationId, existingLevel,
                                   true); //make it additive rather than overwriting, just in case
                           }

                       }
                   }
               }

               //run xtriggers on stacks: repopulate that stack with the replacing element
               var xTriggers = eachStack.GetXTriggers();
                

               foreach (var triggerKey in xTriggers.Keys)
                   //for each XTrigger in the stack, check if any of the aspects present in all the recipe's stacks match the trigger key
                   if (aspectsPresent.ContainsKey(triggerKey))
                   {
                       Element effectElement = _compendium.GetElementById(xTriggers[triggerKey]);
                       if (effectElement == null)
                       {
                           NoonUtility.Log(
                               "Tried to run an xtrigger with an element effect id that doesn't exist: " +
                               xTriggers[triggerKey], 1);
                           return;
                       }

                       {
                           string newElementId = xTriggers[triggerKey];
                           string oldElementId = eachStack.EntityId;
                           int existingQuantity = eachStack.Quantity;

                           eachStack.Populate(newElementId, existingQuantity, Source.Existing());


                           NoonUtility.Log("xtrigger aspect " + triggerKey + " caused " + oldElementId +
                                           " to transform into " +
                                           newElementId, 10);

                       }
                   }
               

           }
       }


   }
}
