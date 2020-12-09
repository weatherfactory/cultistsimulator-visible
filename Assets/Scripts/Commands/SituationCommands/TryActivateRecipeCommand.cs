using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.States;
using Assets.CS.TabletopUI;

namespace Assets.Scripts.Commands.SituationCommands
{
    public class TryActivateRecipeCommand: ISituationCommand
    {
        private readonly Recipe _recipeToActivate;
            
        public CommandCategory CommandCategory => CommandCategory.VerbSlots;

        public TryActivateRecipeCommand(Recipe recipeToActivate)
        {
            _recipeToActivate = recipeToActivate;
        }
        public bool Execute(Situation situation)
        {
            var aspects = situation.GetAspectsAvailableToSituation(true);
            var tc = Registry.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(aspects);

            if (_recipeToActivate.RequirementsSatisfiedBy(aspectsInContext))
            {
                situation.CurrentPrimaryRecipe = _recipeToActivate;
                situation.TimeRemaining = _recipeToActivate.Warmup;

                SoundManager.PlaySfx("SituationBegin");

                //called here in case starting slots trigger consumption
                foreach (var t in situation.GetSpheresByCategory(SphereCategory.Threshold))
                    t.ActivatePreRecipeExecutionBehaviour();

                //now move the stacks out of the starting slots into storage
                situation.AcceptTokens(SphereCategory.SituationStorage,
                    situation.GetElementTokens(SphereCategory.Threshold));

                situation.TransitionToState(new OngoingState());

                return true;
            }

            else
                return false;

        }
    }
}
