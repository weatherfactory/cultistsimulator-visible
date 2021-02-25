using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Constants;
using SecretHistories.Spheres;
using SecretHistories.UI;


namespace SecretHistories.States
{
   public abstract class SituationState
   {
       public abstract void Enter(Situation situation);
       public abstract void Exit(Situation situation);
       public abstract bool IsActiveInThisState(Sphere sphereToCheck);

        public abstract bool IsValidPredictionForState(Recipe recipeToCheck, Situation s);
       public abstract bool AllowDuplicateVerbIfVerbSpontaneous { get; }
       public abstract StateEnum RehydrationValue { get; }

       public bool IsVisibleInThisState(SituationDominion situationDominion)
       {
           return situationDominion.VisibleFor(RehydrationValue);
        }

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
                rehydratedState = new NullSituationState();
                break;
            }

          
            return rehydratedState;
       }


        public abstract void Continue(Situation situation);

   }
}
