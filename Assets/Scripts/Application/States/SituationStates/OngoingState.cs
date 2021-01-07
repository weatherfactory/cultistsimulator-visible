﻿using System;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Constants;

namespace SecretHistories.States
{
    public class OngoingState : SituationState
    {

        public override bool AllowDuplicateVerbIfTransient => false;

        public override void Enter(Situation situation)
        {

        }

        public override void Exit(Situation situation)
        {
            
        }


        public override bool IsActiveInThisState(Sphere s)
        {
            if (s.SphereCategory == SphereCategory.SituationStorage)
                return true;

            //at time of writing, there's only a SlotSpecification if it's a Threshold
            if (s.SphereCategory == SphereCategory.Threshold && s.GoverningSlotSpecification.IsActiveInState(StateEnum.Ongoing))
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
            if(s.CurrentPrimaryRecipe.Alt.Exists(r => r.Id == recipeToCheck.Id && r.ShouldAlwaysSucceed() && !r.Additional))
                return true;

            return false;

        }

        public override void Continue(Situation situation)
        {
            situation.CommandQueue.ExecuteCommandsFor(CommandCategory.Anchor, situation);
            situation.CommandQueue.ExecuteCommandsFor(CommandCategory.RecipeSlots, situation);
            situation.CommandQueue.ExecuteCommandsFor(CommandCategory.Timer, situation);

            if (situation.TimeRemaining <= 0)
             situation.TransitionToState(new RequiresExecutionState());
            else
            {
                situation.TimeRemaining -= situation.IntervalForLastHeartbeat;
                situation.NotifySubscribersOfTimerValueUpdate();
            }
            
        }
    }
}