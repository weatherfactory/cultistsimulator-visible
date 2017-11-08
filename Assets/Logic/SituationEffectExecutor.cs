using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
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


            RunXTriggers(stacksManager, aspectsPresent);
            //note: standard effects happen *after* XTrigger effects
            RunDeckEffect(command,stacksManager,storage);
            //and after deck effect
            RunRecipeEffects(command, stacksManager);
        }

        private void RunDeckEffect(ISituationEffectCommand command, IElementStacksManager stacksManager,IGameEntityStorage storage)
        {
            var deckId = command.GetDeckEffect();
            if(deckId!=null)
            { 
            var deck = storage.DeckInstances.SingleOrDefault(d=>d.Id==deckId);
            if (deck != null)
            {
                var drawnElementId = deck.Draw();

                if(drawnElementId!=null)
                    stacksManager.ModifyElementQuantity(drawnElementId,1);
                //else
                //do nothing, yet. Exhausted decks just stay empty.
            }
            }
        }

        private static void RunRecipeEffects(ISituationEffectCommand command, IElementStacksManager stacksManager)
        {
            foreach (var kvp in command.GetElementChanges())
            {
                stacksManager.ModifyElementQuantity(kvp.Key, kvp.Value);
            }
        }

        private static void RunXTriggers(IElementStacksManager stacksManager, AspectsDictionary aspectsPresent)
        {
            foreach (var eachStack in stacksManager.GetStacks())
            {
                var xTriggers = eachStack.GetXTriggers();
                foreach (var triggerKey in xTriggers.Keys)
                    //for each XTrigger in the stack, check if any of the aspects present in all the recipe's stacks match the trigger key
                    if (aspectsPresent.ContainsKey(triggerKey))
                    {
                        //if they do:
                        var existingQuantity = eachStack.Quantity;
                        //replace the element that has the trigger with the trigger result
                        //eg, if an individual has a Recruiting: individual_b trigger, and there's a Recruiting aspect in the stack, then replace the individual with individual_b
                        stacksManager.ModifyElementQuantity(eachStack.Id, -existingQuantity);
                        stacksManager.ModifyElementQuantity(xTriggers[triggerKey], existingQuantity);

                        NoonUtility.Log("xtrigger aspect " + triggerKey + " caused " + eachStack.Id + " to transform into " +
                                        xTriggers[triggerKey]);
                    }
            }
        }
    }
}
