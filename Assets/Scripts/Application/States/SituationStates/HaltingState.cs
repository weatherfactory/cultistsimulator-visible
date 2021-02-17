﻿using System;
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
        public override StateEnum RehydrationValue => StateEnum.Halting;

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

        public override bool IsVisibleInThisState(Dominion dominion)
        {
            return dominion.VisibleFor(StateEnum.Halting);

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