using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.States;
using Assets.CS.TabletopUI;

namespace Assets.Scripts.Commands.SituationCommands
{
    public class TryHaltSituationCommand : ISituationCommand
    {

            
        public CommandCategory CommandCategory => CommandCategory.Timer;

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