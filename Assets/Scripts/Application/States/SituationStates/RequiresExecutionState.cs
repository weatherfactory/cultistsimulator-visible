using System;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;
using SecretHistories.Fucine;
using Assets.Logic;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Core;
using SecretHistories.Constants;
using SecretHistories.Spheres;

namespace SecretHistories.States
{
    public class RequiresExecutionState : SituationState
    {

        public override bool AllowDuplicateVerbIfVerbSpontaneous => false;
        public override StateEnum Identifier => StateEnum.RequiringExecution;
        public override bool UpdatePredictionDynamically => false;

        public override void Enter(Situation situation)
        {

           
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
            //Situation is RequiringExecution, and recipe is in Linked list of current recipe.  ActionId doesn't need to match.
            if (s.Recipe.Linked.Exists(r => r.Id == recipeToCheck.Id))
                return true;

            return false;
        }

        public override void Continue(Situation situation)
        {
            //This almost certainly needs tweaking to a command-based approach. Although in fact, we might want to reconsider replacing this whole state with commands.
            situation.ExecuteCurrentRecipe();


            var tc = Watchman.Get<HornedAxe>();
            var aspectsInContext = tc.GetAspectsInContext(situation.GetAspects(true));

            var rc = new RecipeConductor(aspectsInContext, Watchman.Get<Stable>().Protag());

            var linkedRecipe = rc.GetLinkedRecipe(situation.Recipe);

            if (linkedRecipe != null)
            {
                var note = new Notification(linkedRecipe.Label, linkedRecipe.StartDescription);

                var addNoteCommand=new AddNoteCommand(note, new Context(Context.ActionSource.UI));
                situation.ExecuteTokenEffectCommand(addNoteCommand);
                
                 situation.SetRecipeActive(linkedRecipe);
                
                if (situation.TimeRemaining > 0) //don't play a sound if we loop through multiple linked ones
                {
                    if (situation.Recipe.SignalImportantLoop)
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