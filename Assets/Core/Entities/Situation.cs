using System;
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
       private Recipe recipe { get; set; }
        public float TimeRemaining { private set; get; }
        public float Warmup { private set; get; }
        public string RecipeId { get { return recipe == null ? null : recipe.Id; } }
        private HashSet<ISituationSubscriber> subscribers=new HashSet<ISituationSubscriber>();

        public Situation(Recipe recipe)
        {
            beginRecipe(recipe);
        }

        public void Subscribe(ISituationSubscriber s)
        {
            subscribers.Add(s);
        }

        private void beginRecipe(Recipe withRecipe)
        {
            recipe = withRecipe;
            Warmup = this.recipe.Warmup;
            TimeRemaining = Warmup;
            State = SituationState.Ongoing;
        }

        public void TryBeginRecipe(IRecipeConductor rc)
        {
            Recipe nextRecipe = rc.GetNextRecipe(recipe);
            if (nextRecipe != null)
                beginRecipe(nextRecipe);
            else
                Extinct();
        }


        public string GetTitle()
        {
            return recipe==null ?  "no recipe just now" :
            recipe.Label;
        }

        public string GetDescription()
        {
            return recipe == null ? "no recipe just now" :
            recipe.Description;
        }

        public SituationState Continue(float interval)
        {
            if(State==SituationState.Complete)
            { 
                Extinct();
            }
            else if (State == SituationState.Ongoing && TimeRemaining <= 0)
            { 
                Complete();
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

        private void Complete()
        {
            State = SituationState.Complete;
            foreach (var s in subscribers)
                s.SituationCompletes(GetEffectCommand());
        }


        private void Extinct()
        {
            recipe = null;
            State = SituationState.Extinct;
            foreach (var s in subscribers)
                s.SituationExtinct();
        }



        private EffectCommand GetEffectCommand()
        {
            Assert.AreEqual(SituationState.Complete,State);
            
                EffectCommand ec=new EffectCommand(recipe.Effects);
                ec.Title = recipe.Label + " complete!";
                ec.Description = recipe.Description;
                return ec;
        }
    }

}
