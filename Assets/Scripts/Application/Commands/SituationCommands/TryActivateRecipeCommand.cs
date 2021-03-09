﻿using System.Collections.Generic;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.States;
using SecretHistories.UI;
using UnityEditorInternal;

namespace SecretHistories.Commands.SituationCommands
{
    public class TryActivateRecipeCommand: ISituationCommand
    {
        private readonly string _recipeId;
        private readonly List<StateEnum> _validForStates=new List<StateEnum>();

        protected TryActivateRecipeCommand(string recipeId, List<StateEnum> validForStates)
        {
            _recipeId = recipeId;
            _validForStates.AddRange(validForStates);

        }

        public static TryActivateRecipeCommand ManualRecipeActivation(string recipeId)
        {
            return new TryActivateRecipeCommand(recipeId,new List<StateEnum>(){ StateEnum.Unstarted });
        }
        public static TryActivateRecipeCommand LinkedRecipeActivation(string recipeId)
        {
            return new TryActivateRecipeCommand(recipeId, new List<StateEnum>() { StateEnum.RequiringExecution,StateEnum.Ongoing });

        }

        public bool IsValidForState(StateEnum forState)
        {
            return forState == StateEnum.Unstarted;
        }

        public bool Execute(Situation situation)
        {
            var aspects = situation.GetAspects(true);
            var tc = Watchman.Get<HornedAxe>();
            var aspectsInContext = tc.GetAspectsInContext(aspects);

            var recipeToActivate = Watchman.Get<Compendium>().GetEntityById<Recipe>(_recipeId);


            if (recipeToActivate.RequirementsSatisfiedBy(aspectsInContext))
            {
                situation.ActivateRecipe(recipeToActivate);
                situation.UpdateCurrentRecipePrediction(situation.GetRecipePredictionForCurrentStateAndAspects(),new Context(Context.ActionSource.SituationEffect));
                situation.TransitionToState(new OngoingState());

            }

            return true; //this is a *try* command. So it always counts as executed when we've tried to activate it, successful or not.
        }
    }
}
