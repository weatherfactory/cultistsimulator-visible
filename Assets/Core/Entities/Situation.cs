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

        public Situation(Recipe recipe)
        {
            this.recipe = recipe;
            Warmup = this.recipe.Warmup;
            TimeRemaining = Warmup;
            State=SituationState.Ongoing;
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
