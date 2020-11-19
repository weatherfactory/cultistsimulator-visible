using System;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.TabletopUi.Scripts.Infrastructure;

namespace Assets.Core.States
{
    public class HaltingState : SituationState
    {
        public override bool Extinct => true;

        protected override void Enter(Situation situation)
        {
            //If we leave anything in the ongoing slot, it's lost, so let's rescue it to SituationStorage
            situation.AcceptTokens(SphereCategory.SituationStorage, situation.GetTokens(SphereCategory.Threshold));
        }

        protected override void Exit(Situation situation)
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
           ChangeState(this,new CompleteState(), situation);
        }
    }
}