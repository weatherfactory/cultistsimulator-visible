using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;

namespace Assets.Core.States
{
    public class UnstartedState : SituationState
    {

        protected override void Enter(Situation situation)
        {
            situation.Reset();
         
        }

        protected override void Exit(Situation situation)
        {
       situation.CurrentRecipePrediction = situation.GetUpdatedRecipePrediction();

            var storageContainer = situation.GetSingleSphereByCategory(SphereCategory.SituationStorage);
            storageContainer.AcceptTokens(situation.GetTokens(SphereCategory.Threshold),
                new Context(Context.ActionSource.SituationStoreStacks));
        }

     

        protected override SituationState GetNextState(Situation situation)
        {
            if (situation.CurrentSituationInterruptCommand.Start)
            {
                var aspects =situation.GetAspectsAvailableToSituation(true);
                var tc = Registry.Get<SphereCatalogue>();
                var aspectsInContext = tc.GetAspectsInContext(aspects);


                var recipe = Registry.Get<ICompendium>().GetFirstMatchingRecipe(aspectsInContext, situation.Verb.Id, Registry.Get<Character>(), false);

                //no recipe found? get outta here
                if (recipe == null)
                    return this;

                situation.CurrentPrimaryRecipe = recipe;
                situation.TimeRemaining = situation.CurrentPrimaryRecipe.Warmup;

                SoundManager.PlaySfx("SituationBegin");

                //called here in case starting slots trigger consumption
                foreach (var t in situation.GetSpheresByCategory(SphereCategory.Threshold))
                    t.ActivatePreRecipeExecutionBehaviour();

                //now move the stacks out of the starting slots into storage
                situation.AcceptTokens(SphereCategory.SituationStorage, situation.GetElementTokens(SphereCategory.Threshold));

                return new OngoingState();

            }

                
            else
                return this;
        }
    }
}