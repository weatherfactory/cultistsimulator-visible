using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.States;

namespace SecretHistories.Commands.SituationCommands
{
   public class AddVerbSlotCommand: ISituationCommand
    {
        public CommandCategory CommandCategory { get; }
        public bool Execute(Situation situation)
        {
            throw new NotImplementedException();
        }
    }
}
