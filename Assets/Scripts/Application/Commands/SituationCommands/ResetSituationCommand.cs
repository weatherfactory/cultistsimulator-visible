using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;

namespace SecretHistories.Assets.Scripts.Application.Commands.SituationCommands
{
 public  class ResetSituationCommand: ISituationCommand
    {
        public bool Execute(Situation situation)
        {
           situation.Recipe = Recipe.CreateSpontaneousHintRecipe(situation.Verb);
           situation.UpdateCurrentRecipePrediction(RecipePrediction.DefaultFromVerb(situation.Verb), new Context(Context.ActionSource.SituationReset));
           situation.SetTimelessShadow();
            

            situation.NotifyStateChange();
            situation.NotifyTimerChange();

            return true;
        }

        public bool IsValidForState(StateEnum forState)
        {
            return forState == StateEnum.Unstarted;
        }
    }
}
