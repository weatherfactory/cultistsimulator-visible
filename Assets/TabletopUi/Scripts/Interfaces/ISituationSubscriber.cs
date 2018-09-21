using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISituationSubscriber
    {
        void SituationBeginning(Recipe withRecipe);
        void SituationOngoing();
        void SituationExecutingRecipe(ISituationEffectCommand situationEffectCommand);
        void SituationComplete();
        void ResetSituation();
        void Halt();
        void ReceiveAndRefineTextNotification(INotification notification);
    }
}
