using System;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;
using SecretHistories.Interfaces;
using Assets.Logic;
using SecretHistories.Core;
using SecretHistories.Constants;
using SecretHistories.Spheres;

namespace SecretHistories.States
{
    public class RequiresExecutionState : SituationState
    {

        public override bool AllowDuplicateVerbIfTransient => false;

        public override void Enter(Situation situation)
        {

            situation.ExecuteCurrentRecipe();
        }

        public override void Exit(Situation situation)
        {
            var outputTokens = situation.GetTokens(SphereCategory.SituationStorage);
            situation.AcceptTokens(SphereCategory.Output, outputTokens, new Context(Context.ActionSource.SituationResults));

            situation.AttemptAspectInductions(situation.CurrentPrimaryRecipe, outputTokens);
            SoundManager.PlaySfx("SituationComplete"); //this could run through that Echo obj

        }

        public override bool IsActiveInThisState(Sphere s)
        {
            return false;
        }

        public override bool IsVisibleInThisState(Dominion dominion)
        {
            return dominion.VisibleFor(StateEnum.RequiringExecution);

        }

        public override bool IsValidPredictionForState(Recipe recipeToCheck, Situation s)
        {
            //Situation is RequiringExecution, and recipe is in Linked list of current recipe.  ActionId doesn't need to match.
            if (s.CurrentPrimaryRecipe.Linked.Exists(r => r.Id == recipeToCheck.Id))
                return true;

            return false;
        }

        public override void Continue(Situation situation)
        {

            var tc = Watchman.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(situation.GetAspectsAvailableToSituation(true));

            var rc = new RecipeConductor(aspectsInContext, Watchman.Get<Character>());

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

                situation.TransitionToState( new OngoingState());

            }
            else
            {
                situation.TransitionToState( new CompleteState());

            }


        }
    }
}