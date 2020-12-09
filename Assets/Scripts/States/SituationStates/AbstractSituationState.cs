using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;

namespace Assets.Core.States
{
   public abstract class SituationState
   {
       public abstract void Enter(Situation situation);
       public abstract void Exit(Situation situation);
       public abstract bool IsActiveInThisState(Sphere sphereToCheck);
       public abstract bool IsValidPredictionForState(Recipe recipeToCheck, Situation s);
       public abstract bool Extinct { get; }


        public static SituationState Rehydrate(StateEnum stateEnum,Situation situation)
       {
           SituationState rehydratedState;


            switch (stateEnum)
           {

                case StateEnum.Unstarted:
                    rehydratedState=new UnstartedState();
                    break;
                    

                case StateEnum.Ongoing:
                    rehydratedState = new OngoingState();
                        break;
                
                case StateEnum.RequiringExecution:
                    rehydratedState = new RequiresExecutionState();
                    break;

                case StateEnum.Halting:
                    rehydratedState = new HaltingState();
                    break;

                case StateEnum.Complete:
                    rehydratedState = new CompleteState();
                    break;

            default:
                    throw new ArgumentOutOfRangeException(nameof(stateEnum),stateEnum, stateEnum.ToString() + " isn't rehydratable");
            }

          
            return rehydratedState;
       }


        public abstract void Continue(Situation situation);




   }
}
