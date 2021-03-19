using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.States;

namespace SecretHistories.Assets.Scripts.Application.States.SituationStates
{
    public class InchoateState: SituationState
    {
        public override void Enter(Situation situation)
        {
            //
        }

        public override void Exit(Situation situation)
        {
            //
        }

        public override bool IsActiveInThisState(Sphere sphereToCheck)
        {
            return false;
        }

        public override bool IsValidPredictionForState(Recipe recipeToCheck, Situation s)
        {
            return true;
        }

        public override bool AllowDuplicateVerbIfVerbSpontaneous => false;
        public override StateEnum Identifier => StateEnum.Inchoate;
        public override void Continue(Situation situation)
        {
            situation.TransitionToState(new UnstartedState());
        }
    }
}
