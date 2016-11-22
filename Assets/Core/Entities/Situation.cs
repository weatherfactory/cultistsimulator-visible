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
    public class Situation
    {
        public SituationState State { get; set; }
       private Recipe currentPrimaryRecipe { get; set; }
        public float TimeRemaining { private set; get; }
        public float Warmup { private set; get; }
        public string RecipeId { get { return currentPrimaryRecipe == null ? null : currentPrimaryRecipe.Id; } }

        public IList<SlotSpecification> GetSlots()
        {
            if (currentPrimaryRecipe.ChildSlotSpecifications.Any())
                return currentPrimaryRecipe.ChildSlotSpecifications;
            else
                return new List<SlotSpecification>();
        }
        private HashSet<ISituationSubscriber> subscribers=new HashSet<ISituationSubscriber>();

        public Situation(Recipe primaryRecipe)
        {
            currentPrimaryRecipe = primaryRecipe;
            Warmup = primaryRecipe.Warmup;
            TimeRemaining = primaryRecipe.Warmup;
            State = SituationState.Unstarted;
        }
        public Situation(float timeRemaining, SituationState state, Recipe withPrimaryRecipe)
        {
            currentPrimaryRecipe = withPrimaryRecipe;
            Warmup = withPrimaryRecipe.Warmup;
            TimeRemaining = timeRemaining;
            State = state;
        }


        public void Subscribe(ISituationSubscriber s)
        {
            subscribers.Add(s);
        }


        public string GetTitle()
        {
            return currentPrimaryRecipe==null ?  "no recipe just now" :
            currentPrimaryRecipe.Label;
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
            else if (State == SituationState.Unstarted)
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
                s.SituationContinues(this);
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
