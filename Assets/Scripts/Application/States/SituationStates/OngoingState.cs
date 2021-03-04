using System;
using System.Collections.Generic;
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
        public override StateEnum RehydrationValue => StateEnum.Ongoing;


        public override void Enter(Situation situation)
        {
            var recipeSlotsCommand = new PopulateDominionCommand("RecipeThresholdsDominion",situation.Recipe.Slots);
            situation.CommandQueue.AddCommand(recipeSlotsCommand);
            var sphereSpec=new SphereSpec(typeof(SituationStorageSphere), new StorageSphereIdStrategy());
            var storageCommand = new PopulateDominionCommand("StorageDominion",sphereSpec);
                situation.CommandQueue.AddCommand(storageCommand);

                var migrateFromVerbSlotsToStorageCommand=new FlushTokensToCategoryCommand(SphereCategory.Threshold,SphereCategory.SituationStorage,StateEnum.Ongoing);
                situation.CommandQueue.AddCommand(migrateFromVerbSlotsToStorageCommand);
            
                SoundManager.PlaySfx("SituationBegin");
        }

        public override void Exit(Situation situation)
        {

            var migrateFromRecipeSlotsToStorageComand = new FlushTokensToCategoryCommand(SphereCategory.Threshold, SphereCategory.SituationStorage, StateEnum.RequiringExecution);
            situation.CommandQueue.AddCommand(migrateFromRecipeSlotsToStorageComand);
            
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