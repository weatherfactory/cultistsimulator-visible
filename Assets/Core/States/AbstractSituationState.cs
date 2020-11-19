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
   public abstract class SituationState: IEquatable<SituationState>
   {
       protected abstract void Enter(Situation situation);
       protected abstract void Exit(Situation situation);
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

            rehydratedState.Enter(situation);
            return rehydratedState;
       }


       public SituationState Continue(Situation situation)
       {
           var newState = this.GetNextState(situation);
           this.Exit(situation);
           newState.Enter(situation);;
           return newState;
       }

       protected abstract SituationState GetNextState(Situation situation);


       public bool Equals(SituationState other)
       {
           throw new NotImplementedException();
       }

       public override bool Equals(object obj)
       {
           return ReferenceEquals(this, obj) || obj is SituationState other && Equals(other);
       }

       public override int GetHashCode()
       {
           throw new NotImplementedException();
       }

       public static bool operator ==(SituationState left, SituationState right)
       {
           return Equals(left, right);
       }

       public static bool operator !=(SituationState left, SituationState right)
       {
           return !Equals(left, right);
       }
   }
}
