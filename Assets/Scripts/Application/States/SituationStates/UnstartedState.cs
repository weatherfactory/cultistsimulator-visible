﻿using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Infrastructure;

namespace SecretHistories.States
{
    public class UnstartedState : SituationState
    {

        public override void Enter(Situation situation)
        {
            situation.Reset();
        }

        public override void Exit(Situation situation)
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

        public override bool Extinct => false;


        public override void Continue (Situation situation)
        {
            situation.CommandQueue.ExecuteCommandsFor(CommandCategory.VerbSlots,situation);
        }
    }
}