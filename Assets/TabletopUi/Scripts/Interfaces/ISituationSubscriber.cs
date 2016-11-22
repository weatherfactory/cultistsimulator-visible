using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISituationSubscriber
    {
        void SituationBeginning(Situation s);
        void SituationContinues(Situation s);
        void SituationExecutingRecipe(IEffectCommand effectCommand);
        void SituationExtinct();


    }
}
