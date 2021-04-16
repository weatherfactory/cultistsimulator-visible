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


        public override void Enter(Situation situation)
        {
            if(!situation.GetSpheresByCategory(SphereCategory.SituationStorage).Any()) //create storage sphere if none exists already. *Don't* create if it does - we might have looped
            //straight back in here from a linked recipe, in which the existing storage sphere probably contains cardsn we don't want to flush.
            {
                var sphereSpec=new SphereSpec(typeof(SituationStorageSphere), nameof(SituationStorageSphere));
                var storageCommand = new PopulateDominionCommand(SituationDominionEnum.Storage.ToString(),sphereSpec);
                situation.AddCommand(storageCommand);
            }
            var migrateFromVerbSlotsToStorageCommand=new FlushTokensToCategoryCommand(SphereCategory.Threshold,SphereCategory.SituationStorage,StateEnum.Ongoing);
                situation.AddCommand(migrateFromVerbSlotsToStorageCommand);


                var recipeSlotsCommand = new PopulateDominionCommand(SituationDominionEnum.RecipeThresholds.ToString(), situation.Recipe.Slots);
                situation.AddCommand(recipeSlotsCommand);

            SoundManager.PlaySfx("SituationBegin");
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


        /// <summary>
        /// WARNING: this assumes ShouldAlwaysSucceed, which is greast for prediction but not for execution
        /// </summary>
        /// <param name="currentRecipe"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override bool IsValidPredictionForState(Recipe recipeToCheck, Situation s)
        {

            //Situation is Ongoing. Recipe is in Alt list of current recipe - as Always Succeed and not as Additional. ActionId doesn't need to match.
            if(s.Recipe.Alt.Exists(r => r.Id == recipeToCheck.Id && r.ShouldAlwaysSucceed() && !r.Additional))
                return true;

            return false;

        }

        public override void Continue(Situation situation)
        {
       
            if (situation.TimeRemaining <= 0)
             situation.TransitionToState(new RequiresExecutionState());
            else
            {
                situation.ReduceLifetimeBy(situation.IntervalForLastHeartbeat);
                situation.NotifyTimerChange();
            }
            
        }
    }
}