using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Noon;

namespace Assets.Core.States
{
   public abstract class SituationState: IEquatable<SituationState>
   {
       protected abstract void Enter(Situation situation);
       protected abstract void Exit(Situation situation);


       public static SituationState Rehydrate(StateEnum stateEnum,Situation situation)
       {
           SituationState rehydrationState;


            switch (stateEnum)
           {

                case StateEnum.Unstarted:
                    rehydrationState=new UnstartedState();
                    break;
                    
                case StateEnum.Ongoing:
                    rehydrationState = new OngoingState();
                        break;
                
                case StateEnum.RequiringExecution:
                    rehydrationState = new RequiresExecutionState();
                    break;

                case StateEnum.Complete:
                    rehydrationState = new CompleteState();
                    break;

            default:
                    throw new ArgumentOutOfRangeException(stateEnum.ToString());
            }

            rehydrationState.Enter(situation);
            return rehydrationState;
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


   public class CompleteState : SituationState
   {

        protected override void Enter(Situation situation)
       {
         

           var outputTokens = situation.GetTokens(SphereCategory.SituationStorage);
           situation.AcceptTokens(SphereCategory.Output, outputTokens, new Context(Context.ActionSource.SituationResults));

           situation.AttemptAspectInductions(situation.CurrentPrimaryRecipe, outputTokens);
           SoundManager.PlaySfx("SituationComplete"); //this could run through that Echo obj
        }

       protected override void Exit(Situation situation)
       {
           throw new NotImplementedException();
       }

       protected override SituationState GetNextState(Situation situation)
       {
           return new UnstartedState();
       }
   }



   public class HaltingState : SituationState
   {
       protected override void Enter(Situation situation)
       {
           situation.Complete();

            //If we leave anything in the ongoing slot, it's lost, and also the situation ends up in an anomalous state which breaks loads
            situation.AcceptTokens(SphereCategory.SituationStorage, situation.GetTokens(SphereCategory.Threshold));
        }

       protected override void Exit(Situation situation)
       {
           throw new NotImplementedException();
       }

       protected override SituationState GetNextState(Situation situation)
       {
           throw new NotImplementedException();
       }
   }



}
