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
            return true; //HaltSituation will try to execute in all states.
            //It only affects OngoingState, but we don't want it to sit in the queue until it finds a state it's valid for.
        }

        public bool IsObsoleteInState(StateEnum forState)
        {
            return false;
        }


        public TryHaltSituationCommand()
        {

        }
        public bool Execute(Situation situation)
        {
            //halt any ongoing situations
            if(situation.State.Identifier==StateEnum.Ongoing)
                situation.TransitionToState( new HaltingState());
            
            //return true in any case, so we remove from queue.
            //There was a bug where a Halt command would wait on an unstarted verb and then pounce when it began executing, which
            //particularly caused problems with the wrong spheres being present afterwards.
            return true;
        }
    }
}