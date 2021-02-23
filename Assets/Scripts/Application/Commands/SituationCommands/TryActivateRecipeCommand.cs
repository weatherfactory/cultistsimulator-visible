using System.Collections.Generic;
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
        private readonly Recipe _recipeToActivate;
            
        
        public TryActivateRecipeCommand(Recipe recipeToActivate)
        {
            _recipeToActivate = recipeToActivate;
        }

        public List<StateEnum> ValidForStates=>new List<StateEnum> {StateEnum.Unstarted};

        public bool Execute(Situation situation)
        {
            var aspects = situation.GetAspects(true);
            var tc = Watchman.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(aspects);

            if (_recipeToActivate.RequirementsSatisfiedBy(aspectsInContext))
            {
                situation.ActivateRecipe(_recipeToActivate);
                situation.UpdateCurrentRecipePrediction(situation.GetRecipePredictionForCurrentStateAndAspects(),new Context(Context.ActionSource.SituationEffect));
                situation.TransitionToState(new OngoingState());

                return true;
            }

            else
                return false;

        }
    }
}
