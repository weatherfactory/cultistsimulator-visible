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
        private HashSet<ISituationSubscriber> subscribers=new HashSet<ISituationSubscriber>();

        public Situation(Recipe recipe)
        {
            InitialiseWithRecipe(recipe);
        }

        public void InitialiseWithRecipe(Recipe withRecipe)
        {
            Initialise(withRecipe.Warmup,SituationState.Ongoing, withRecipe);
        }

        public Situation(float timeRemaining, SituationState state, Recipe withRecipe)
        {
            Initialise(timeRemaining, state, withRecipe);
        }


        public void Initialise(float timeRemaining, SituationState state, Recipe withRecipe)
        {
            currentRecipe = withRecipe;
            Warmup = withRecipe.Warmup;
            TimeRemaining = timeRemaining;
            State = state;
            foreach (var s in subscribers)
                s.SituationContinues();
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
            else
            {
                TimeRemaining = TimeRemaining - interval;
                Ongoing();

            }
            return State;


        }


        private void Ongoing()
        {
            State=SituationState.Ongoing;
            foreach (var s in subscribers)
                s.SituationContinues();
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
                InitialiseWithRecipe(nextRecipes.Single());
            else
                Extinct();
        }


        private void Extinct()
        {
            State = SituationState.Extinct;
            foreach (var s in subscribers)
                s.SituationExtinct();
        }



        private EffectCommand GetEffectCommand()
        {
            Assert.AreEqual(SituationState.RequiringExecution,State);
            
                EffectCommand ec=new EffectCommand(currentRecipe.Effects);
                ec.Title = currentRecipe.Label + " complete!";
                ec.Description = currentRecipe.Description;
                return ec;
        }
    }

}
