using System;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Constants;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.States
{
    public class OngoingState : SituationState
    {

        public override bool AllowDuplicateVerbIfVerbSpontaneous => false;
        public override StateEnum Identifier => StateEnum.Ongoing;
        public override bool UpdatePredictionDynamically => true;

        public override void Enter(Situation situation)
        {
            if(!situation.GetSpheresByCategory(SphereCategory.SituationStorage).Any()) //create storage sphere if none exists already. *Don't* create if it does - we might have looped
            //straight back in here from a linked recipe, in which the existing storage sphere probably contains cardsn we don't want to flush.
            {
                CreateStorageSlot(situation);
            }

            FlushFromThresholdsToStorage(situation);
            PopulateRecipeSlots(situation);
            SoundManager.PlaySfx("SituationBegin");
        }

        private static void CreateStorageSlot(Situation situation)
        {
            var sphereSpec = new SphereSpec(typeof(SituationStorageSphere), nameof(SituationStorageSphere));
            var storageCommand = new PopulateDominionCommand(SituationDominionEnum.Storage.ToString(), sphereSpec);
            situation.AddCommand(storageCommand);
        }

        private static void FlushFromThresholdsToStorage(Situation situation)
        {
            var flushFromThresholdsToStorage = new FlushTokensToCategoryCommand(SphereCategory.Threshold,
                SphereCategory.SituationStorage, StateEnum.Ongoing);
            situation.AddCommand(flushFromThresholdsToStorage);
        }

        private static void PopulateRecipeSlots(Situation situation)
        {
            //we've just moved to Ongoing, so either it's a brand new recipe or a linked recipe.
            //in either case, it should override or clear any existing slots.

            if (situation.Recipe.Slots.Any())
            {
                var recipeSlotsCommand =
                    new PopulateDominionCommand(SituationDominionEnum.RecipeThresholds.ToString(), situation.Recipe.Slots);
                situation.AddCommand(recipeSlotsCommand);
            }
            else
            {
                var clearDominionCommand = new ClearDominionCommand(SituationDominionEnum.RecipeThresholds.ToString(),
                    SphereRetirementType.Graceful);
                situation.AddCommand(clearDominionCommand);
            }
        }

        public override void Exit(Situation situation)
        {

            var migrateFromRecipeSlotsToStorageComand = new FlushTokensToCategoryCommand(SphereCategory.Threshold, SphereCategory.SituationStorage, StateEnum.RequiringExecution);
            situation.AddCommand(migrateFromRecipeSlotsToStorageComand);
            
        }

        public override bool IsActiveInThisState(Sphere s)
        {
            if (s.SphereCategory == SphereCategory.SituationStorage)
                return true;

            //at time of writing, there's only a SlotSpecification if it's a Threshold
            if (s.SphereCategory == SphereCategory.Threshold && s.GoverningSphereSpec.IsActiveInState(StateEnum.Ongoing))
                return true;

            return false;
        }

        public override List<Recipe> PotentiallyPredictableRecipesForState(Situation s)
        {
            //Situation is Ongoing. Recipe is in Alt list of current recipe - as Always Succeed and not as Additional. ActionId doesn't need to match.
            /// WARNING: this assumes ShouldAlwaysSucceed, which is great for prediction but not for execution
            List<Recipe> potentiallyValidRecipes = new List<Recipe>();
            foreach (var a in s.Recipe.Alt.Where(altLink=>altLink.ShouldAlwaysSucceed() && !altLink.Additional))
            {
                var potentiallyValidRecipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(a.Id);
                if(potentiallyValidRecipe.IsValid()) //no null/verb recipes in Ongoing
                    potentiallyValidRecipes.Add(potentiallyValidRecipe);
            }

            return potentiallyValidRecipes;
        }


        public override void Continue(Situation situation)
        {
       
            if (situation.TimeRemaining <= 0)
             situation.TransitionToState(new RequiresExecutionState());
            else
            {
                if(situation.IntervalForLastHeartbeat>0) //don't trigger all those subscriber events unless time has actually passed for the situation
                {
                    situation.ReduceLifetimeBy(situation.IntervalForLastHeartbeat);
                    situation.NotifyTimerChange();
                }
            }
            
        }
    }
}