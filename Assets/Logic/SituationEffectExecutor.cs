using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Infrastructure;
using JetBrains.Annotations;
using Noon;
using UnityEngine;

namespace Assets.Logic
{
    public class SituationEffectExecutor
    {
        private TabletopManager _ttm;

        public SituationEffectExecutor(TabletopManager ttm)
        {
            _ttm = ttm;
        }

        public void RunEffects(ISituationEffectCommand command, ITokenContainer tokenContainer,
            Character storage, IDice d)
        {
            var recipeAspects = command.Recipe.Aspects;
            var cardAspects = tokenContainer.GetTotalAspects();
            IDice dice = d;

            //MutationEffects happen first. I often regret this, because I sometimes create a card and want to apply a mutationeffect to it.
            //But I can always pass something through an empty recipe if I gotta.
            RunMutationEffects(command, tokenContainer);


            //note: xtriggers for recipe aspects happen before xtriggers for card aspects. Within that precedence, aspects take effect in non-specific order.
            //I think this will generally make sense. Recipe aspects are 'specifically, I want to do this'
            //Card aspects are 'this should generally happen'
            //If this basic logic doesn't work, solutions under consideration: (1) xtrigger priorities (2) feeding a stack back in if it's transformed to react to its new xtriggers (with guard against loop)

            RunXTriggers(tokenContainer, recipeAspects, dice);
            RunXTriggers(tokenContainer, cardAspects, dice);

            //note: standard effects happen *after* XTrigger effects
            RunDeckEffect(command, tokenContainer, storage);
            //and after deck effect
            RunRecipeEffects(command, tokenContainer);

            //Penultimate: run verb manipulations and element purges. This means purges will occur *after* any elements have been mutated or xtrigger-transformed.

            RunVerbManipulations(command, _ttm);
            //Element purges are run after verb manipulations. This is so we can halt a verb and then delete any applicable contents (rather than deleting the verb, which is possible but very risky if it contains plot-relevant elements!)
            RunElementPurges(command, _ttm);

            //Do this last: remove any stacks marked for consumption by being placed in a consuming slot
            RunConsumptions(
                tokenContainer); //NOTE: If a stack has just been transformed into another element, all sins are forgiven. It won't be consumed.
        }


        private void RunElementPurges(ISituationEffectCommand command, TabletopManager ttm)
        {
            //NOTE: element purges trigger decayto transformation if the element itself is specified. If we filter by aspect and purge on that, its decayto is *not* triggered.
            foreach (var p in command.Recipe.Purge)
            {
                ttm.PurgeElement(p.Key, p.Value);
            }
        }


        private void RunVerbManipulations(ISituationEffectCommand command, TabletopManager ttm)
        {
            foreach (var h in command.Recipe.HaltVerb)
                ttm.HaltVerb(h.Key, h.Value);

            foreach (var d in command.Recipe.DeleteVerb)
                ttm.DeleteVerb(d.Key, d.Value);
        }


        private void RunMutationEffects(ISituationEffectCommand command, ITokenContainer tokenContainer)
        {
            foreach (var mutationEffect in command.Recipe.Mutations)
            {
                foreach (var stack in tokenContainer.GetStacks())
                {
                    if (stack.GetAspects(true).ContainsKey(mutationEffect.Filter))
                        stack.SetMutation(mutationEffect.Mutate, mutationEffect.Level,
                            mutationEffect.Additive);
                }
            }
        }

        private void RunConsumptions(ITokenContainer tokenContainer)
        {

            var stacks = tokenContainer.GetStacks();

            for (int i = 0; i < stacks.Count(); i++)
            {
                if (stacks.ElementAt(i) != null && stacks.ElementAt(i).MarkedForConsumption)
                {
                    stacks.ElementAt(i).Retire(CardVFX.CardBurn);
                }
            }
        }

        public void RunDeckEffect(ISituationEffectCommand command, ITokenContainer tokenContainer,
            Character storage)
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
                            var drawnCardId = deck.Draw();

                            if (!string.IsNullOrEmpty(drawnCardId))
                            {
                                var source = Source.Fresh(); //ultimately this should correspond to deck
                                tokenContainer.ModifyElementQuantity(drawnCardId, 1, source,
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

        private static void RunRecipeEffects(ISituationEffectCommand command, ITokenContainer tokenContainer)
        {
            foreach (var kvp in command.GetElementChanges())
            {

                if (!int.TryParse(kvp.Value, out var effectValue))
                {
                    //it's a string not an int, so it must be a reference to a quantity of another element
                    effectValue = tokenContainer.GetTotalAspects(true).AspectValue(kvp.Value);
                }

                var source = Source.Fresh(); //might later be eg Transformed
                tokenContainer.ModifyElementQuantity(kvp.Key, effectValue, source,
                    new Context(Context.ActionSource.SituationEffect));
            }
        }

        private static void RunXTriggers(ITokenContainer tokenContainer, AspectsDictionary aspectsPresent,
            IDice dice)
        {
            ICompendium _compendium = Registry.Get<ICompendium>();

            foreach (var eachStack in tokenContainer.GetStacks())
            {
                RunXTriggersOnMutationsForStack(tokenContainer, aspectsPresent, dice, eachStack, _compendium);


                RunXTriggersOnStackItself(tokenContainer, aspectsPresent, dice, eachStack, _compendium);
            }
        }

        private static void RunXTriggersOnMutationsForStack(ITokenContainer tokenContainer, [CanBeNull] AspectsDictionary aspectsPresent, [CanBeNull] IDice dice,
            [CanBeNull] ElementStackToken eachStack, [CanBeNull] ICompendium _compendium)
        {
            foreach (var eachStackMutation in eachStack.GetCurrentMutations())
            {
                //first we apply xtriggers to mutations - but not to the default stack aspects.
                //i.e., mutations can get replaced by other mutations thanks to xtrigger morphs
                //we do this first in the expectation that the stack will generally get repopulated next, if there's a relevant xtrigger
                var stackMutationBaseAspect = _compendium.GetEntityById<Element>(eachStackMutation.Key);
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
                                        tokenContainer.ProvisionElementStack(newElementId, morph.Level,
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

        private static void RunXTriggersOnStackItself(ITokenContainer tokenContainer, AspectsDictionary aspectsPresent,
            IDice dice, ElementStackToken eachStack, ICompendium _compendium)
        {
            var xTriggers = eachStack.GetXTriggers();

            foreach (var triggerKey in xTriggers.Keys)
            {
                //for each XTrigger in the stack, check if any of the aspects present in all the recipe's stacks match the trigger key
                if (aspectsPresent.ContainsKey(triggerKey))
                {
                    foreach (var morph in xTriggers[triggerKey])
                    {
                        Element effectElement = _compendium.GetEntityById<Element>(morph.Id);
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
                              tokenContainer.ProvisionElementStack(newElementId, morph.Level,
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

