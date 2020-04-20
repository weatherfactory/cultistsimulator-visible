using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using JetBrains.Annotations;
using Noon;
using TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Logic
{
    public class SituationEffectExecutor
    {
        private ITabletopManager _ttm;

        public SituationEffectExecutor(ITabletopManager ttm)
        {
            _ttm = ttm;
        }

        public void RunEffects(ISituationEffectCommand command, IElementStacksManager stacksManager,
            IGameEntityStorage storage, IDice d)
        {
            var recipeAspects = command.Recipe.Aspects;
            var cardAspects = stacksManager.GetTotalAspects();
            IDice dice = d;

            //MutationEffects happen first. I often regret this, because I sometimes create a card and want to apply a mutationeffect to it.
            //But I can always pass something through an empty recipe if I gotta.
            RunMutationEffects(command, stacksManager);


            //note: xtriggers for recipe aspects happen before xtriggers for card aspects. Within that precedence, aspects take effect in non-specific order.
            //I think this will generally make sense. Recipe aspects are 'specifically, I want to do this'
            //Card aspects are 'this should generally happen'
            //If this basic logic doesn't work, solutions under consideration: (1) xtrigger priorities (2) feeding a stack back in if it's transformed to react to its new xtriggers (with guard against loop)

            RunXTriggers(stacksManager, recipeAspects, dice);
            RunXTriggers(stacksManager, cardAspects, dice);

            //note: standard effects happen *after* XTrigger effects
            RunDeckEffect(command, stacksManager, storage);
            //and after deck effect
            RunRecipeEffects(command, stacksManager);

            //Penultimate: run purges and verb manipulations. This means purges will occur *after* any elements have been mutated or xtrigger-transformed.
             RunPurges(command, _ttm);

            RunVerbManipulations(command, _ttm); 

            //Do this last: remove any stacks marked for consumption by being placed in a consuming slot
            RunConsumptions(
                stacksManager); //NOTE: If a stack has just been transformed into another element, all sins are forgiven. It won't be consumed.
        }


        private void RunPurges(ISituationEffectCommand command, ITabletopManager ttm)
        {
            foreach (var p in command.Recipe.Purge)
            {
                ttm.PurgeElement(p.Key, p.Value);
            }
        }



        private void RunVerbManipulations(ISituationEffectCommand command, ITabletopManager ttm)
        {
            foreach (var h in command.Recipe.HaltVerb)
                ttm.HaltVerb(h.Key, h.Value);

            foreach (var d in command.Recipe.DeleteVerb)
                ttm.DeleteVerb(d.Key, d.Value);
        }


        private void RunMutationEffects(ISituationEffectCommand command, IElementStacksManager stacksManager)
        {
            foreach (var mutationEffect in command.Recipe.MutationEffects)
            {
                foreach (var stack in stacksManager.GetStacks())
                {
                    if (stack.GetAspects(true).ContainsKey(mutationEffect.FilterOnAspectId))
                        stack.SetMutation(mutationEffect.MutateAspectId, mutationEffect.MutationLevel,
                            mutationEffect.Additive);
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
                    stacks.ElementAt(i).Retire(CardVFX.CardBurn);
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

                            if (!string.IsNullOrEmpty(drawnCardId))
                            {
                                var source = Source.Fresh(); //ultimately this should correspond to deck
                                stacksManager.ModifyElementQuantity(drawnCardId, 1, source,
                                    new Context(Context.ActionSource.SituationEffect));
                            }
                            else
                            {
                                Debug.LogError($"Failed to draw card '{drawnCardId}' from deck '{deckId}'");
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
                stacksManager.ModifyElementQuantity(kvp.Key, kvp.Value, source,
                    new Context(Context.ActionSource.SituationEffect));
            }
        }

        private static void RunXTriggers(IElementStacksManager stacksManager, AspectsDictionary aspectsPresent,
            IDice dice)
        {
            ICompendium _compendium = Registry.Retrieve<ICompendium>();

            foreach (var eachStack in stacksManager.GetStacks())
            {
                RunXTriggersOnMutationsForStack(stacksManager, aspectsPresent, dice, eachStack, _compendium);


                RunXTriggersOnStackItself(stacksManager, aspectsPresent, dice, eachStack, _compendium);
            }
        }

        private static void RunXTriggersOnMutationsForStack(IElementStacksManager stacksManager,[CanBeNull] AspectsDictionary aspectsPresent, [CanBeNull] IDice dice,
            [CanBeNull] IElementStack eachStack, [CanBeNull] ICompendium _compendium)
        {
            foreach (var eachStackMutation in eachStack.GetCurrentMutations())
            {
                //first we apply xtriggers to mutations - but not to the default stack aspects.
                //i.e., mutations can get replaced by other mutations thanks to xtrigger morphs
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
                            foreach (var morph in mutationXTrigger.Value)
                            {
                                if (morph.Chance >= dice.Rolld100())
                                {
                                    string newElementId = morph.Id;
                                    string currentMutationId = eachStackMutation.Key;
                                    int existingLevel = eachStackMutation.Value;

                                    if (morph.MorphEffect == MorphEffectType.Transform)
                                    {
                                        eachStack.SetMutation(currentMutationId, 0, false);
                                        eachStack.SetMutation(newElementId, existingLevel * morph.Level,
                                            true); //make it additive rather than overwriting, just in case
                                    }
                                    else if (morph.MorphEffect == MorphEffectType.Spawn)
                                    {
                                        stacksManager.AddAndReturnStack(newElementId, morph.Level,
                                            Source.Existing(),
                                            new Context(Context.ActionSource.ChangeTo));
                                        NoonUtility.Log(
                                            "xtrigger aspect marked additional=true " + mutationXTrigger + " caused " +
                                            currentMutationId + " to spawn a new " + newElementId);
                                    }
                                    else if(morph.MorphEffect==MorphEffectType.Mutate) 
                                    {
                                        eachStack.SetMutation(newElementId, morph.Level, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void RunXTriggersOnStackItself(IElementStacksManager stacksManager, AspectsDictionary aspectsPresent,
            IDice dice, IElementStack eachStack, ICompendium _compendium)
        {
            var xTriggers = eachStack.GetXTriggers();

            foreach (var triggerKey in xTriggers.Keys)
            {
                //for each XTrigger in the stack, check if any of the aspects present in all the recipe's stacks match the trigger key
                if (aspectsPresent.ContainsKey(triggerKey))
                {
                    foreach (var morph in xTriggers[triggerKey])
                    {
                        Element effectElement = _compendium.GetElementById(morph.Id);
                        if (effectElement == null)
                        {
                            NoonUtility.Log(
                                "Tried to run an xtrigger with an element effect id that doesn't exist: " +
                                xTriggers[triggerKey]);
                        }

                        else if (morph.Chance >= dice.Rolld100())
                        {
                            string newElementId = morph.Id;
                            string oldElementId = eachStack.EntityId;
                            int existingQuantity = eachStack.Quantity;
                            if (morph.MorphEffect==MorphEffectType.Transform)
                            {
                                eachStack.Populate(newElementId, existingQuantity, Source.Existing());
                                NoonUtility.Log(
                                    "Transform xtrigger " + triggerKey + " caused " + oldElementId +
                                    " to transform into " + newElementId);
                            }
                            else if (morph.MorphEffect == MorphEffectType.Spawn)
                            {
                                stacksManager.AddAndReturnStack(newElementId, morph.Level,
                                    Source.Existing(),
                                    new Context(Context.ActionSource.ChangeTo));
                                NoonUtility.Log(
                                    "Spawn xtrigger " + triggerKey + " caused " +
                                    oldElementId + " to spawn a new " + newElementId);
                            }
                            else if (morph.MorphEffect == MorphEffectType.Mutate)
                            {
                                eachStack.SetMutation(newElementId, morph.Level, true);
                            }
                        }
                    }
                }
            }
        }
    }
}

