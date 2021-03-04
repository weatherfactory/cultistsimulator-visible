using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.States;
using SecretHistories.UI;

namespace SecretHistories.Commands.SituationCommands
{
    public class TryHaltSituationCommand : ISituationCommand
    {
        public bool IsValidForState(StateEnum forState)
        {
            return forState == StateEnum.Ongoing;
        }


        public TryHaltSituationCommand()
        {

        }
        public bool Execute(Situation situation)
        {
          situation.TransitionToState( new HaltingState());

          return true;
        }
    }
}