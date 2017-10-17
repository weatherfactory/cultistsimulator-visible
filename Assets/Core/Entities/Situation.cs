using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using Assets.Logic;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Interfaces;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace Assets.Core.Entities
{
    public interface ISituation
    {
        SituationState State { get; set; }
        float TimeRemaining { get; }
        float Warmup { get; }
        string RecipeId { get; }
        
        IList<SlotSpecification> GetSlotsForCurrentRecipe();

        string GetTitle();
        string GetStartingDescription();
        string GetDescription();
        SituationState Continue(IRecipeConductor rc,float interval);
        RecipePrediction GetPrediction(IRecipeConductor rc);
        void Beginning(Recipe withRecipe);
        void Start(Recipe primaryRecipe);
        void ResetIfComplete();
    }

    public class Situation : ISituation
    {
        public global::SituationState State { get; set; }
       private Recipe currentPrimaryRecipe { get; set; }
        public float TimeRemaining { private set; get; }
        public float Warmup { get { return currentPrimaryRecipe.Warmup; } }
        public string RecipeId { get { return currentPrimaryRecipe == null ? null : currentPrimaryRecipe.Id; } }
        private ISituationSubscriber subscriber;
   

        public IList<SlotSpecification> GetSlotsForCurrentRecipe()
        {
            if (currentPrimaryRecipe.SlotSpecifications.Any())
                return currentPrimaryRecipe.SlotSpecifications;
            else
                return new List<SlotSpecification>();
        }
        

        public Situation(ISituationSubscriber s)
        {
            subscriber = s;
            State = SituationState.Unstarted;
        }

        private void Reset()
        {
            currentPrimaryRecipe = null;
            TimeRemaining = 0;
            State = SituationState.Unstarted;
            subscriber.SituationHasBeenReset();
        }

        public void Start(Recipe primaryRecipe)
        {
            currentPrimaryRecipe = primaryRecipe;
            TimeRemaining = primaryRecipe.Warmup;
            State = global::SituationState.FreshlyStarted;
            SoundManager.PlaySfx("SituationBegin");
        }

        public void ResetIfComplete()
        {
            if(State==SituationState.Complete)
                Reset();
        }



        public Situation(float? timeRemaining, global::SituationState state, Recipe withPrimaryRecipe,ISituationSubscriber s)
        {
            subscriber = s;
            currentPrimaryRecipe = withPrimaryRecipe;
            TimeRemaining = timeRemaining ?? 0;
            State = state;

        }




        public string GetTitle()
        {
            return currentPrimaryRecipe==null ?  "no recipe just now" :
            currentPrimaryRecipe.Label;
        }

        public string GetStartingDescription()
        {
            return currentPrimaryRecipe == null ? "no recipe just now" :
      currentPrimaryRecipe.StartDescription;
        }

        public string GetDescription()
        {
            return currentPrimaryRecipe == null ? "no recipe just now" :
            currentPrimaryRecipe.Description;
        }


        public SituationState Continue(IRecipeConductor rc,float interval)
        {

      
            if (State == SituationState.RequiringExecution)
            {
                End(rc);
            }
            else if (State == SituationState.Ongoing && TimeRemaining <= 0)
            {
                RequireExecution(rc);
            }
            else if (State == SituationState.FreshlyStarted)
            {
                Beginning(currentPrimaryRecipe);
            }
            else if (State == SituationState.Unstarted || State==SituationState.Complete)
            {
                //do nothing: it's either not running, or it's finished running and waiting for user action
            }
            else if(State==SituationState.Ongoing)
            {
                TimeRemaining = TimeRemaining - interval;
                Ongoing();

            }
            return State;
        }

        public RecipePrediction GetPrediction(IRecipeConductor rc)
        {
            var rp = rc.GetRecipePrediction(currentPrimaryRecipe);

            return rp;

        }

        public void Beginning(Recipe withRecipe)
        {
            State=SituationState.Ongoing;
           subscriber.SituationBeginning(withRecipe);
        }


        private void Ongoing()
        {
            State=SituationState.Ongoing;
            subscriber.SituationOngoing();
        }

        private void RequireExecution(IRecipeConductor rc)
        {
            State = SituationState.RequiringExecution;

            IList<Recipe> recipesToExecute = rc.GetActualRecipesToExecute(currentPrimaryRecipe);

            //actually replace the current recipe with the first on the list: any others will be additionals,
            //but we want to loop from this one.
            if (recipesToExecute.First().Id != currentPrimaryRecipe.Id)
                currentPrimaryRecipe = recipesToExecute.First();

                foreach (var r in recipesToExecute)
                {
                    ISituationEffectCommand ec=new SituationEffectCommand(r, r.ActionId!=currentPrimaryRecipe.ActionId);
                    subscriber.SituationExecutingRecipe(ec);
                }
            
        }

        private void End(IRecipeConductor rc)
        {

            SoundManager.PlaySfx("SituationComplete");

            var loopedRecipe = rc.GetLoopedRecipe(currentPrimaryRecipe);
            
            if (loopedRecipe!=null)
            { 
                currentPrimaryRecipe = loopedRecipe;
                TimeRemaining = currentPrimaryRecipe.Warmup;
                Beginning(currentPrimaryRecipe);
            }
            else
                Complete();
        }


        private void Complete()
        {
            State = global::SituationState.Complete;
            subscriber.SituationComplete();
        }

    }

}
