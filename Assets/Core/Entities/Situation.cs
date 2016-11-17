using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


        public SituationState Continue(float interval)
        {
return SituationState.Ongoing;
        }

        private void Ongoing()
        {
            State=SituationState.Ongoing;
            foreach (var s in subscribers)
                s.SituationContinues();
        }
        //the problem: we want to fire the recipe as soon as we're done.
        //but we also want to wait to be supplied with information on what to execute.
        //The situation is a state machine that Does Things, so it should Execute Whatever when it executes
        //we should pass the recipe conductor with continue, to give it the information it needs to make that decision
        //it will then execute all the alternative recipes (but keep the original recipe)
        //this means Continue while Ending replaces our TryGetNextRecipe in tests
        //and we can set the state now!
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
            currentRecipe = null;
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
