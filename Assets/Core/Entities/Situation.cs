using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    public class Situation
    {
        public SituationState State { get; set; }
       private Recipe recipe { get; set; }
        public float TimeRemaining { private set; get; }
        public float Warmup { private set; get; }
        public string RecipeId { get { return recipe == null ? null : recipe.Id; } }

        public Situation(Recipe recipe)
        {
            beginRecipe(recipe);
        }

        private void beginRecipe(Recipe withRecipe)
        {
            recipe = withRecipe;
            Warmup = this.recipe.Warmup;
            TimeRemaining = Warmup;
            State = SituationState.Ongoing;
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
                State=SituationState.Extinct;
            else if (State == SituationState.Ongoing && TimeRemaining <= 0)
                State = SituationState.Complete;
            else
                TimeRemaining = TimeRemaining - interval;
            return State;
        }

        public void TryBeginNextRecipe(IRecipeConductor rc)
        {
            Recipe nextRecipe = rc.GetNextRecipe(recipe);
            if (nextRecipe != null) 
              beginRecipe(nextRecipe);
        }

        public EffectCommand GetEffectCommand()
        {
            if (State == SituationState.Complete)
            {
                EffectCommand ec=new EffectCommand(recipe.Effects);
                ec.Title = recipe.Label + " complete!";
                ec.Description = recipe.Description;
                return ec;
            }
            return null;
        }
    }

}
