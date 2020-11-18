using System;
using Assets.Core.Entities;

namespace Assets.Core.States
{
    public class OngoingState : SituationState
    {

        protected override void Enter(Situation situation)
        {
         situation.CurrentBeginningEffectCommand = new RecipeBeginningEffectCommand(situation.CurrentPrimaryRecipe.Slots, situation.CurrentRecipePrediction.BurnImage);
        }

        protected override void Exit(Situation situation)
        {
            throw new NotImplementedException();
        }

        protected override SituationState GetNextState(Situation situation)
        {
            if (situation.CurrentSituationInterruptCommand.Halt)
                return new HaltingState();

            
            if (situation.TimeRemaining <= 0)
                return new RequiresExecutionState();
            else
            {
                situation.TimeRemaining = situation.TimeRemaining - situation.IntervalForLastHeartbeat;
                return this;
            }
            
        }
    }
}