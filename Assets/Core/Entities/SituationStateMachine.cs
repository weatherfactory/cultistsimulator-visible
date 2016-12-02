using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Logic;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine.Assertions;

namespace Assets.Core.Entities
{
    public interface ISituationStateMachine
    {
        SituationState State { get; set; }
        float TimeRemaining { get; }
        float Warmup { get; }
        string RecipeId { get; }
        IList<SlotSpecification> GetSlots();
        void Subscribe(ISituationStateMachineSituationSubscriber s);
        string GetTitle();
        string GetStartingDescription();
        string GetDescription();
        AspectMatchFilter GetRetrievalFilter();
        SituationState Continue(IRecipeConductor rc,float interval);
        string GetPrediction(IRecipeConductor rc);
        void Beginning();
    }

    public class SituationStateMachine : ISituationStateMachine
    {
        public SituationState State { get; set; }
       private Recipe currentPrimaryRecipe { get; set; }
        public float TimeRemaining { private set; get; }
        public float Warmup { get { return currentPrimaryRecipe.Warmup; } }
        public string RecipeId { get { return currentPrimaryRecipe == null ? null : currentPrimaryRecipe.Id; } }

        public IList<SlotSpecification> GetSlots()
        {
            if (currentPrimaryRecipe.SlotSpecifications.Any())
                return currentPrimaryRecipe.SlotSpecifications;
            else
                return new List<SlotSpecification>();
        }
        private HashSet<ISituationStateMachineSituationSubscriber> subscribers=new HashSet<ISituationStateMachineSituationSubscriber>();

        public SituationStateMachine(Recipe primaryRecipe)
        {
            currentPrimaryRecipe = primaryRecipe;
            TimeRemaining = primaryRecipe.Warmup;
            State = SituationState.Fresh;
        }
        public SituationStateMachine(float timeRemaining, SituationState state, Recipe withPrimaryRecipe)
        {
            currentPrimaryRecipe = withPrimaryRecipe;
            TimeRemaining = timeRemaining;
            State = state;
        }


        public void Subscribe(ISituationStateMachineSituationSubscriber s)
        {
            subscribers.Add(s);
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

        public AspectMatchFilter GetRetrievalFilter()
        {
            return new AspectMatchFilter(currentPrimaryRecipe.RetrievesContentsWith);
        }

        public SituationState Continue(IRecipeConductor rc,float interval)
        {

            if (State == SituationState.Ending)
            {
                Extinct();
            }
            if (State == SituationState.RequiringExecution)
            {
                End(rc);
            }
            else if (State == SituationState.Ongoing && TimeRemaining <= 0)
            {
                RequireExecution(rc);
            }
            else if (State == SituationState.Fresh)
            {
                Beginning();
            }
            else
            {
                TimeRemaining = TimeRemaining - interval;
                Ongoing();

            }
            return State;
        }

        public string GetPrediction(IRecipeConductor rc)
        {
            IList<Recipe> recipes= rc.GetActualRecipesToExecute(currentPrimaryRecipe);
            string desc = "";
            foreach (var r in recipes)
                desc = desc + r.Label + " (" + r.Description + "); ";

            return "Next: " + desc;

        }

        public void Beginning()
        {
            State=SituationState.Ongoing;
            foreach (var s in subscribers)
                s.SituationBeginning(this);
        }


        private void Ongoing()
        {
            State=SituationState.Ongoing;
            foreach (var s in subscribers)
                s.SituationOngoing(this);
        }

        private void RequireExecution(IRecipeConductor rc)
        {
            State = SituationState.RequiringExecution;

            IList<Recipe> recipesToExecute = rc.GetActualRecipesToExecute(currentPrimaryRecipe);

            //actually replace the current recipe with the first on the list: any others will be additionals,
            //but we want to loop from this one.
            //We should probably return a command with a subsidiary list, not a basic list
            if (recipesToExecute.First().Id != currentPrimaryRecipe.Id)
                currentPrimaryRecipe = recipesToExecute.First();


            foreach (var s in subscribers)
            {
                foreach (var r in recipesToExecute)
                {
                    s.SituationExecutingRecipe(new EffectCommand(r));
                }
            }
        }

        private void End(IRecipeConductor rc)
        {
            State=SituationState.Ending;

            var loopedRecipe = rc.GetLoopedRecipe(currentPrimaryRecipe);
            if (loopedRecipe!=null)
            { 
                currentPrimaryRecipe = loopedRecipe;
                TimeRemaining = currentPrimaryRecipe.Warmup;
                Beginning();
            }
            else
                Extinct();
        }


        private void Extinct()
        {
            State = SituationState.Extinct;
            foreach (var s in subscribers)
                s.SituationExtinct();
        }

    }

}
