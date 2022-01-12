using System.Collections.Generic;

using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.States;
using SecretHistories.UI;

namespace SecretHistories.Commands.SituationCommands
{
    public class TryActivateRecipeCommand: ISituationCommand
    {
        private readonly string _recipeId;
        private readonly List<StateEnum> _validForStates=new List<StateEnum>();
        private readonly bool _alwaysActivateRegardlessOfRequirements;

        public TryActivateRecipeCommand()
        {
            //to keep json deserialisation more stable
        }

        protected TryActivateRecipeCommand(string recipeId, List<StateEnum> validForStates, bool alwaysActivateRegardlessOfRequirements)
        {
            _recipeId = recipeId;
            _validForStates.AddRange(validForStates);
            _alwaysActivateRegardlessOfRequirements = alwaysActivateRegardlessOfRequirements;

        }

        public static TryActivateRecipeCommand ManualRecipeActivation(string recipeId)
        {
            return new TryActivateRecipeCommand(recipeId,new List<StateEnum>(){ StateEnum.Unstarted },false);
        }
        public static TryActivateRecipeCommand LinkedRecipeActivation(string recipeId)
        {
            return new TryActivateRecipeCommand(recipeId, new List<StateEnum>() { StateEnum.RequiringExecution,StateEnum.Ongoing },true);

        }


        public static TryActivateRecipeCommand OverridingRecipeActivation(string recipeId)
        {
            return new TryActivateRecipeCommand(recipeId, new List<StateEnum>() { StateEnum.RequiringExecution, StateEnum.Ongoing,StateEnum.Unstarted }, true);

        }

        public bool IsValidForState(StateEnum forState)
        {
            return forState == StateEnum.Unstarted;
        }

        public bool IsObsoleteInState(StateEnum forState)
        {
            return false;
        }

        public bool Execute(Situation situation)
        {
            var aspects = situation.GetAspects(true);
            var tc = Watchman.Get<HornedAxe>();
            var aspectsInContext = tc.GetAspectsInContext(aspects);

            var recipeToActivate = Watchman.Get<Compendium>().GetEntityById<Recipe>(_recipeId);


            if (_alwaysActivateRegardlessOfRequirements || recipeToActivate.RequirementsSatisfiedBy(aspectsInContext))
            {
                situation.SetRecipeActive(recipeToActivate);
                situation.ReactToNewRecipePrediction(situation.GetRecipePredictionForCurrentStateAndAspects(),new Context(Context.ActionSource.SituationEffect));
                situation.TransitionToState(new OngoingState());

            }

            return true; //this is a *try* command. So it always counts as executed when we've tried to activate it, successful or not.
        }
    }
}
