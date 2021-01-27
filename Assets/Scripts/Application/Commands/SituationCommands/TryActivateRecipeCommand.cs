using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.States;
using SecretHistories.UI;

namespace SecretHistories.Commands.SituationCommands
{
    public class TryActivateRecipeCommand: ISituationCommand
    {
        private readonly Recipe _recipeToActivate;
            
        public CommandCategory CommandCategory => CommandCategory.VerbThresholds;

        public TryActivateRecipeCommand(Recipe recipeToActivate)
        {
            _recipeToActivate = recipeToActivate;
        }
        public bool Execute(Situation situation)
        {
            var aspects = situation.GetAspectsAvailableToSituation(true);
            var tc = Watchman.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(aspects);

            if (_recipeToActivate.RequirementsSatisfiedBy(aspectsInContext))
            {
                situation.Recipe = _recipeToActivate;
                situation.TimeRemaining = _recipeToActivate.Warmup;
                situation.CurrentRecipePrediction = situation.GetUpdatedRecipePrediction();

                var storageContainer = situation.GetSingleSphereByCategory(SphereCategory.SituationStorage);

                //now we're safely started on the recipe, consume any tokens in Consuming thresholds
                foreach (var thresholdSphere in situation.GetSpheresByCategory(SphereCategory.Threshold))
                {
                    if (thresholdSphere.GoverningSphereSpec.Consumes)
                        thresholdSphere.RetireAllTokens();
                }

                storageContainer.AcceptTokens(situation.GetTokens(SphereCategory.Threshold),
                    new Context(Context.ActionSource.SituationStoreStacks));

                SoundManager.PlaySfx("SituationBegin");

                situation.TransitionToState(new OngoingState());

                return true;
            }

            else
                return false;

        }
    }
}
