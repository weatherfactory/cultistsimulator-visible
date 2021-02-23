using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.States;

namespace SecretHistories.Commands
{


    
    public interface ISituationCommand
    {
        List<StateEnum> ValidForStates { get; }
        bool Execute(Situation situation);
    }
}
