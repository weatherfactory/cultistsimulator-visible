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
using SecretHistories.Commands.TokenEffectCommands;
using SecretHistories.Core;
using SecretHistories.Constants;
using SecretHistories.Services;
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

        public override List<Recipe> PotentiallyPredictableRecipesForState(Situation s)
        {
            return new List<Recipe>();
            //
            //List<Recipe> potentiallyValidRecipes = new List<Recipe>();
            //foreach (var l in s.Recipe.Linked)
            //{
            //    //Situation is RequiringExecution, and recipe is in Linked list of current recipe.  ActionId doesn't need to match.
            //    var potentiallyValidRecipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(l.Id);
            //    if (potentiallyValidRecipe.IsValid()) //no null/verb recipes in Ongoing
            //        potentiallyValidRecipes.Add(potentiallyValidRecipe);
            //}

            //return potentiallyValidRecipes;
        }

        public override void Continue(Situation situation)
        {
            //This almost certainly needs tweaking to a command-based approach. Although in fact, we might want to reconsider replacing this whole state with commands.
            situation.ExecuteCurrentRecipe();


            var tc = Watchman.Get<HornedAxe>();
            var aspectsInContext = tc.GetAspectsInContext(situation.GetAspects(true),situation.GetNearbyAspects(true));

            var rc = new RecipeConductor(aspectsInContext, Watchman.Get<Stable>().Protag());

            var linkedRecipe = rc.GetLinkedRecipe(situation.Recipe);

            if (linkedRecipe != null)
            {
                var aspectsInSituation = situation.GetAspects(true);
                TextRefiner tr = new TextRefiner(aspectsInSituation);
                var noteLinkedRecipeIsBeginning = new Notification(linkedRecipe.Label,
                    tr.RefineString(linkedRecipe.StartDescription),true);

                NoonUtility.Log($"Situation notification: {situation.Recipe.Id} has executed and has linked recipe {linkedRecipe.Id}. Sending a notification with startdescription of {linkedRecipe.Id}.",0,VerbosityLevel.Significants);
                var addNoteCommand=new AddNoteToTokenCommand(noteLinkedRecipeIsBeginning, new Context(Context.ActionSource.UI));
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