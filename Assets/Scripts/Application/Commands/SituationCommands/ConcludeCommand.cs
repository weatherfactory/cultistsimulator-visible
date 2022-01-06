using System;

using System.Linq;

using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.States;


namespace SecretHistories.Commands.SituationCommands
{
   public class ConcludeCommand: ISituationCommand
   {
       
       public bool IsValidForState(StateEnum forState)
       {
           return forState == StateEnum.Complete;
       }

       public bool IsObsoleteInState(StateEnum forState)
       {
           //return false;
           return forState == StateEnum.Unstarted;
       }

       public bool Execute(Situation situation)
        {
 

            if(situation.Verb.Spontaneous)
                situation.Retire(RetirementVFX.Default);
            else
                situation.TransitionToState(new UnstartedState());

            return true;
        }
    }
}
