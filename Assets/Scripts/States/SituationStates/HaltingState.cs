using System;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.TabletopUi.Scripts.Infrastructure;

namespace Assets.Core.States
{
    public class HaltingState : SituationState
    {
        public override bool Extinct => true;

        public override void Enter(Situation situation)
        {
            //If we leave anything in the ongoing slot, it's lost, so let's rescue it to SituationStorage
            situation.AcceptTokens(SphereCategory.SituationStorage, situation.GetTokens(SphereCategory.Threshold));
            situation.Reset();
        }

        public override void Exit(Situation situation)
        {
            //
        }

        public override bool IsActiveInThisState(Sphere s)
        {
            return false;
        }

        public override bool IsValidPredictionForState(Recipe recipeToCheck, Situation s)
        {
            return false;
        }

        public override void Continue(Situation situation)
        {
            situation.TransitionToState(new CompleteState());
        }
    }
}