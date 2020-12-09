using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.States;

namespace Assets.Scripts.Commands.SituationCommands
{
   public class ConcludeCommand: ISituationCommand
   {
       public CommandCategory CommandCategory => CommandCategory.Output;

        public bool Execute(Situation situation)
        {
            situation.TransitionToState(new UnstartedState());
            return true;
        }
    }
}
