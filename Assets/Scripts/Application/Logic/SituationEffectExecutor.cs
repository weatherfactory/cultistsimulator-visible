using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Constants;
using JetBrains.Annotations;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Spheres;
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

        public void RunEffects(RecipeCompletionEffectCommand command, Sphere sphere,
            Character storage, IDice d)
        {
            var recipeAspects = command.Recipe.Aspects;
            var cardAspects = sphere.GetTotalAspects();
            IDice dice = d;

            //MutationEffects happen first. I often regret this, because I sometimes create a card and want to apply a mutationeffect to it.
            //But I can always pass something through an empty recipe if I gotta.
            RunMutationEffects(command, sphere);


            //note: xtriggers for recipe aspects happen before xtriggers for card aspects. Within that precedence, aspects take effect in non-specific order.
            //I think this will generally make sense. Recipe aspects are 'specifically, I want to do this'
            //Card aspects are 'this should generally happen'
            //If this basic logic doesn't work, solutions under consideration: (1) xtrigger priorities (2) feeding a stack back in if it's transformed to react to its new xtriggers (with guard against loop)

            RunXTriggers(sphere, recipeAspects, dice);
            RunXTriggers(sphere, cardAspects, dice);

            //note: standard effects happen *after* XTrigger effects
            RunDeckEffect(command, sphere, storage);
            //and after deck effect
            RunRecipeEffects(command, sphere);

            //Penultimate: run verb manipulations and element purges. This means purges will occur *after* any elements have been mutated or xtrigger-transformed.

            RunVerbManipulations(command, _ttm);
            //Element purges are run after verb manipulations. This is so we can halt a verb and then delete any applicable contents (rather than deleting the verb, which is possible but very risky if it contains plot-relevant elements!)
            RunElementPurges(command, _ttm);

        }


        private void RunElementPurges(RecipeCompletionEffectCommand command, TabletopManager ttm)
        {
            //NOTE: element purges trigger decayto transformation if the element itself is specified. If we filter by aspect and purge on that, its decayto is *not* triggered.
            foreach (var p in command.Recipe.Purge)
            {
                Watchman.Get<SphereCatalogue>().PurgeElement(p.Key, p.Value);
            }
        }


        private void RunVerbManipulations(RecipeCompletionEffectCommand command, TabletopManager ttm)
        {
            foreach (var h in command.Recipe.HaltVerb)
                Watchman.Get<SituationsCatalogue>().HaltSituation(h.Key, h.Value);

            foreach (var d in command.Recipe.DeleteVerb)
                Watchman.Get<SituationsCatalogue>().DeleteSituation(d.Key, d.Value);
        }


        private void RunMutationEffects(RecipeCompletionEffectCommand command, Sphere sphere)
        {
            foreach (var mutationEffect in command.Recipe.Mutations)
            {
                foreach (var token in sphere.GetElementTokens())
                {
                    if (token.GetAspects(true).ContainsKey(mutationEffect.Filter))
                        token.Payload.SetMutation(mutationEffect.Mutate, mutationEffect.Level,
                            mutationEffect.Additive);
                }
            }
        }


        public void RunDeckEffect(RecipeCompletionEffectCommand command, Sphere sphere,
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
                                sphere.ModifyElementQuantity(drawnCardId, 1, new Context(Context.ActionSource.SituationEffect));
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

        private static void RunRecipeEffects(RecipeCompletionEffectCommand command, Sphere sphere)
        {
            foreach (var kvp in command.GetElementChanges())
            {

                if (!int.TryParse(kvp.Value, out var effectValue))
                {
                    //it's a string not an int, so it must be a reference to a quantity of another element
                    effectValue = sphere.GetTotalAspects(true).AspectValue(kvp.Value);
                }

                sphere.ModifyElementQuantity(kvp.Key, effectValue, new Context(Context.ActionSource.SituationEffect));
            }
        }

        private static void RunXTriggers(Sphere sphere, AspectsDictionary aspectsPresent,
            IDice dice)
        {
            Compendium _compendium = Watchman.Get<Compendium>();

            ITokenEffectCommand xTriggerCommand=new XTriggerCommand(aspectsPresent,dice,sphere);

            foreach (var eachToken in sphere.GetElementTokens())
            {
                eachToken.ExecuteTokenEffectCommand(xTriggerCommand);
            }
        }

     

    }
}

