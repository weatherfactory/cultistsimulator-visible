using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISituationSubscriber
    {
        void SituationBeginning(SituationEventData e);
        void SituationOngoing(SituationEventData e);
        void SituationExecutingRecipe(SituationEventData e);
        void SituationComplete(SituationEventData e);
        void ResetSituation();
        void ContainerContentsUpdated(SituationEventData e);
        void ReceiveAndRefineTextNotification(SituationEventData e);
    }
}
