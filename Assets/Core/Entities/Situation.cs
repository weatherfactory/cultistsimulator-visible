using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    public class Situation
    {
        public SituationState State { get; set; }
       private Recipe Recipe { get; set; }
        public float TimeRemaining { private set; get; }
        public float Warmup { private set; get; }

        public Situation(Recipe recipe)
        {
            Recipe = recipe;
            Warmup = Recipe.Warmup;
            TimeRemaining = Warmup;
            State=SituationState.Ongoing;
        }

        public SituationState Continue(float interval)
        {
            TimeRemaining = TimeRemaining - interval;
            return State;
        }
    }

}
