﻿using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;

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


            SoundManager.PlaySfx("SituationBegin");

            //called here in case starting slots trigger consumption
            foreach (var t in situation.GetSpheresByCategory(SphereCategory.Threshold))
                t.ActivatePreRecipeExecutionBehaviour();

            //now move the stacks out of the starting slots into storage
         situation.AcceptTokens(SphereCategory.SituationStorage, situation.GetElementTokens(SphereCategory.Threshold));


            
        }

        public override bool IsActiveInThisState(Sphere s)
        {
            if (s.SphereCategory != SphereCategory.Threshold)

                return false;

            return s.GoverningSlotSpecification.IsActiveInState(StateEnum.Unstarted);

        }

        public override bool IsValidPredictionForState(Recipe recipeToCheck,Situation s)
        {
            //return true if:
            //Situation is Unstarted; verb matches; and the recipe is either craftable or hintable
            if ((recipeToCheck.Craftable || recipeToCheck.HintOnly) && recipeToCheck.ActionId == s.Verb.Id)
                return true;

            return false;
        }


        protected override SituationState GetNextState(Situation situation)
        {
            if (situation.CurrentInterruptInputs.Contains(SituationInterruptInput.Start))
            {
                situation.CurrentInterruptInputs.Remove(SituationInterruptInput.Start);
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