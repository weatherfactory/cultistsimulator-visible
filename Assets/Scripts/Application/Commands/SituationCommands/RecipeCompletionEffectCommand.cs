using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Logic;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Core
{

    public class RecipeCompletionEffectCommand: ISituationCommand
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Recipe Recipe { get; set; }
        public bool AsNewSituation { get; set; } //determines whether the recipe will spawn a new situation.
        public Expulsion Expulsion { get; set; }
        public FucinePath ToPath { get; set; }
        
        public RecipeCompletionEffectCommand() : this(NullRecipe.Create(), false, new Expulsion(), FucinePath.Current())
        {}

        public RecipeCompletionEffectCommand(Recipe recipe,bool asNewSituation,Expulsion expulsion, FucinePath toPath)
        {
            Recipe = recipe;
            Title = "default title";
            Description = recipe.Description;
            AsNewSituation = asNewSituation;
            Expulsion = expulsion;
            ToPath = toPath;
        }

        public bool Execute(Situation situation)
        {
            NoonUtility.Log($"EXECUTING: {Recipe.Id}");
            situation.Recipe = this.Recipe;
            var recipeAspects = Recipe.Aspects;
            var targetSphere = situation.GetSingleSphereByCategory(SphereCategory.SituationStorage);
            var aspectsFromContents = targetSphere.GetTotalAspects();
            IDice dice = Watchman.Get<IDice>();

            //MutationEffects happen first. I often regret this, because I sometimes create a card and want to apply a mutationeffect to it.
            //But I can always pass something through an empty recipe if I gotta.
            RunMutationEffects(targetSphere);


            //note: xtriggers for recipe aspects happen before xtriggers for card aspects. Within that precedence, aspects take effect in non-specific order.
            //I think this will generally make sense. Recipe aspects are 'specifically, I want to do this'
            //Card aspects are 'this should generally happen'
            //If this basic logic doesn't work, solutions under consideration: (1) xtrigger priorities (2) feeding a stack back in if it's transformed to react to its new xtriggers (with guard against loop)

            RunXTriggers(targetSphere, recipeAspects, dice);
            RunXTriggers(targetSphere, aspectsFromContents, dice);

            //note: standard effects happen *after* XTrigger effects
            RunDeckEffect(targetSphere);
            //and after deck effect
            RunRecipeEffects(targetSphere);

            //Penultimate: run verb manipulations and element purges. This means purges will occur *after* any elements have been mutated or xtrigger-transformed.

            RunVerbManipulations();
            //Element purges are run after verb manipulations. This is so we can halt a verb and then delete any applicable contents (rather than deleting the verb, which is possible but very risky if it contains plot-relevant elements!)
            RunElementPurges();

            OpenPortals(situation);
            DoRecipeVfx(situation);
            TryHonourSeeking(situation);

            return true;
        }

        private void TryHonourSeeking(Situation situation)
        {
            if (!Recipe.Seeking.Any())
                return;
            if (!situation.Token.Sphere.Traversable)
                return;
            if (situation.Token.CurrentState.InSystemDrivenMotion())
                return;

            if (Recipe.Seeking.Any())
            {
                //find the first token (later, could be nearest token) which matches the seeking criteria
                var matchingTokens = Watchman.Get<HornedAxe>().FindTokensWithAspectsInWorld(Recipe.Seeking);
                if (matchingTokens.Count == 0)
                    return;

                var goToToken = matchingTokens.First();
                NoonUtility.Log($"{situation.Token.PayloadId} setting course for {goToToken.PayloadId}");
              
                var pathItinerary = new TokenPathItinerary(situation.Token.Location, goToToken.Location);
                
                //create a PathItinerary towards that token
                pathItinerary.Depart(situation.Token,new Context(Context.ActionSource.SituationEffect));
                

                //This is immediately flawed in several ways:
                //(1) what happens if the situation token is in motion when the recipe triggers? currently we just discard the seeking, but we could store it as a command.
                //(2) what happens if the target moves while the situation token is on its way to it? There's stuff in AIPath to handle this, so maybe we should let it handle it
                //(3) what happens if we want to trigger a callback when the token arrives, and/or cancel the seeking when the recipe is complete? We can fudge this with a cycling recipe, for now.
                //-- sounds lke a pathreqs, actually...

            }
        }

        private void DoRecipeVfx(Situation situation)
        {
            if(!string.IsNullOrEmpty(Recipe.BurnImage))
            {
                var burnImageCommand=new BurnImageCommand(Recipe.BurnImage);
                situation.AddCommand(burnImageCommand);
            }
        }


        public bool IsValidForState(StateEnum forState)
        {
            return forState == StateEnum.RequiringExecution;
        }

        public bool IsObsoleteInState(StateEnum forState)
        {
            return false;
        }

        private void RunElementPurges()
        {
            //NOTE: element purges trigger decayto transformation if the element itself is specified. If we filter by aspect and purge on that, its decayto is *not* triggered.
            foreach (var p in Recipe.Purge)
            {
                Watchman.Get<HornedAxe>().PurgeElement(p.Key, p.Value);
            }
        }


        private void RunVerbManipulations()
        {
            foreach (var h in Recipe.HaltVerb)
                Watchman.Get<HornedAxe>().HaltSituation(h.Key, h.Value);

            foreach (var d in Recipe.DeleteVerb)
                Watchman.Get<HornedAxe>().PurgeSituation(d.Key, d.Value);
        }


        private void RunMutationEffects(Sphere sphere)
        {
            foreach (var mutationEffect in Recipe.Mutations)
            {
                foreach (var token in sphere.GetElementTokens())
                {
                    if (token.GetAspects(true).ContainsKey(mutationEffect.Filter))
                        token.Payload.SetMutation(mutationEffect.Mutate, mutationEffect.Level,
                            mutationEffect.Additive);
                }
            }
        }


        public void RunDeckEffect(Sphere sphere)
        {
            if (Recipe.DeckEffects != null && Recipe.DeckEffects.Any())
            {
                var dealer = new Dealer(Watchman.Get<DealersTable>());

                foreach (var deckId in Recipe.DeckEffects.Keys)

                    for (int i = 1; i <= Recipe.DeckEffects[deckId]; i++)
                    {
                        {
                            var drawnCard = dealer.Deal(deckId);
                            sphere.AcceptToken(drawnCard, Context.Unknown());
                        }
                    }
            }
        }

        private void RunRecipeEffects(Sphere sphere)
        {
            foreach (var kvp in Recipe.Effects)
            {

                if (!int.TryParse(kvp.Value, out var effectValue))
                {
                    //it's a string not an int, so it must be a reference to a quantity of another element
                    effectValue = sphere.GetTotalAspects(true).AspectValue(kvp.Value);
                }

                sphere.ModifyElementQuantity(kvp.Key, effectValue, new Context(Context.ActionSource.SituationEffect));
            }
        }

        private void RunXTriggers(Sphere sphere, AspectsDictionary aspectsPresent,
            IDice dice)
        {
      
            IAffectsTokenCommand xTriggerCommand = new XTriggerCommand(aspectsPresent, dice, sphere);

            foreach (var eachToken in sphere.GetElementTokens())
            {
                eachToken.ExecuteTokenEffectCommand(xTriggerCommand);
            }
        }

        private void OpenPortals(Situation situation)
        {
            if (!string.IsNullOrEmpty(Recipe.PortalEffect))
            {
                var portalCreationCommand=new IngressCreationCommand(Recipe.PortalEffect.ToString());
                var spawnPortalTokenCommand=new SpawnNewTokenFromThisOneCommand(portalCreationCommand,
               FucinePath.Current(), 
                    Context.Unknown());
                situation.Token.ExecuteTokenEffectCommand(spawnPortalTokenCommand);
            }
        }
    }
}
