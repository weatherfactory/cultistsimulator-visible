using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.States;
using SecretHistories.UI;

namespace Assets.Scripts.Application.Entities.NullEntities
{
    public class NullSituationState: SituationState
    {
        public override bool UpdatePredictionDynamically => false;

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

        public override List<Recipe> PotentiallyValidRecipesForState(Situation s)
        {
            return new List<Recipe>();
        }

        public override bool AllowDuplicateVerbIfVerbSpontaneous => true;

        public override StateEnum Identifier => StateEnum.Inchoate;

        public override void Continue(Situation situation)
        {
          situation.TransitionToState(new UnstartedState());
        }
    }
}
