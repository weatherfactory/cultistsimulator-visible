using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.States;

namespace SecretHistories.Commands
{


    
    public interface ISituationCommand: IEncaustment
    {
   
        bool Execute(Situation situation);
        bool IsValidForState(StateEnum forState);
    }
}
