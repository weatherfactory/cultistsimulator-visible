using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.States;
using SecretHistories.UI;

namespace Assets.Scripts.Application.Entities.NullEntities
{
    public class NullSituationState: SituationState
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

        public override bool IsVisibleInThisState(Dominion dominion)
        {
            return false;

        }

        public override bool IsValidPredictionForState(Recipe recipeToCheck, Situation s)
        {
            return false;

        }

        public override bool AllowDuplicateVerbIfTransient => true;
        public override void Continue(Situation situation)
        {
          //
        }
    }
}
