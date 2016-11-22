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
       private Recipe currentRecipe { get; set; }
        public float TimeRemaining { private set; get; }
        public float Warmup { private set; get; }
        public string RecipeId { get { return currentRecipe == null ? null : currentRecipe.Id; } }

        public IList<ChildSlotSpecification> GetChildSlots()
        {
            if (currentRecipe.ChildSlotSpecifications.Any())
                return currentRecipe.ChildSlotSpecifications;
            else
                return new List<ChildSlotSpecification>();
        }
        private HashSet<ISituationSubscriber> subscribers=new HashSet<ISituationSubscriber>();

        public Situation(Recipe recipe)
        {
            currentRecipe = recipe;
            Warmup = recipe.Warmup;
            TimeRemaining = recipe.Warmup;
            State = SituationState.Unstarted;
        }
        public Situation(float timeRemaining, SituationState state, Recipe withRecipe)
        {
            currentRecipe = withRecipe;
            Warmup = withRecipe.Warmup;
            TimeRemaining = timeRemaining;
            State = state;
        }


        public void Subscribe(ISituationSubscriber s)
        {
            subscribers.Add(s);
        }


        public string GetTitle()
        {
            return currentRecipe==null ?  "no recipe just now" :
            currentRecipe.Label;
        }

        public string GetDescription()
        {
            return currentRecipe == null ? "no recipe just now" :
            currentRecipe.Description;
        }

        public AspectMatchFilter GetRetrievalFilter()
        {
            return new AspectMatchFilter(currentRecipe.RetrievesContentsWith);
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
            IList<Recipe> recipes= rc.GetActualRecipesToExecute(currentRecipe);
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

            IList<Recipe> recipesToExecute = rc.GetActualRecipesToExecute(currentRecipe);

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

            var nextRecipes = rc.GetNextRecipes(currentRecipe);
            if (nextRecipes.Any())
            { 
                currentRecipe = nextRecipes.Single();
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
