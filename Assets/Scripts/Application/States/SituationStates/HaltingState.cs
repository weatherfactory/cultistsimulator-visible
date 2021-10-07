using System;
using SecretHistories.Assets.Scripts.Application.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Constants;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.States
{
    public class HaltingState : SituationState
    {
        public override bool AllowDuplicateVerbIfVerbSpontaneous => true;
        public override StateEnum Identifier => StateEnum.Halting;
        public override bool UpdatePredictionDynamically => false;

        public override void Enter(Situation situation)
        {
            //If we leave anything in the ongoing slot, it's lost, so let's rescue it to SituationStorage
            situation.AcceptTokens(SphereCategory.SituationStorage, situation.GetTokens(SphereCategory.Threshold));
            situation.GetTimeshadow().SpendAllRemainingTime();
        }

        public override void Exit(Situation situation)
        {
           
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