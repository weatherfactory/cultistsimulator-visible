﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Logic;

namespace Assets.Core.States
{
    public class RequiresExecutionState : SituationState
    {

        protected override void Enter(Situation situation)
        {

            situation.ExecuteCurrentRecipe();
        }

        protected override void Exit(Situation situation)
        {
            throw new NotImplementedException();
        }

        protected override SituationState GetNextState(Situation situation)
        {
            if (situation.CurrentSituationInterruptCommand.Halt)
                return new HaltingState();

            var tc = Registry.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(situation.GetAspectsAvailableToSituation(true));

            var rc = new RecipeConductor(aspectsInContext, Registry.Get<Character>());

            var linkedRecipe = rc.GetLinkedRecipe(situation.CurrentPrimaryRecipe);

            if (linkedRecipe != null)
            {
                //send the completion description before we move on
                INotification notification = new Notification(situation.CurrentPrimaryRecipe.Label, situation.CurrentPrimaryRecipe.Description);
                situation.SendNotificationToSubscribers(notification);

                //I think this code duplicates ActivateRecipe, below
                situation.CurrentPrimaryRecipe = linkedRecipe;
                situation.TimeRemaining = situation.CurrentPrimaryRecipe.Warmup;
                if (situation.TimeRemaining > 0) //don't play a sound if we loop through multiple linked ones
                {
                    if (situation.CurrentPrimaryRecipe.SignalImportantLoop)
                        SoundManager.PlaySfx("SituationLoopImportant");
                    else
                        SoundManager.PlaySfx("SituationLoop");

                }

                return new UnstartedState();
            }
            else
            {
                return new CompleteState();
            }


        }
    }
}