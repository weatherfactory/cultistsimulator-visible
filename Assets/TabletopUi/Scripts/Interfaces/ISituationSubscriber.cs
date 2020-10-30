using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISituationSubscriber
    {
        void DisplaySituationState(SituationEventData e);
        void ContainerContentsUpdated(SituationEventData e);
        void ReceiveNotification(SituationEventData e);
        void RecipePredicted(RecipePrediction recipePrediction);
    }
}
