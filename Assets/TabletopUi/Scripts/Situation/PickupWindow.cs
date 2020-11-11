using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Commands;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts
{
   public class PickupWindow: MonoBehaviour,ISituationSubscriber
    {
        public void DisplaySituationState(SituationEventData e)
        {
            throw new NotImplementedException();
        }

        public void ContainerContentsUpdated(SituationEventData e)
        {
            throw new NotImplementedException();
        }

        public void ReceiveNotification(SituationEventData e)
        {
            throw new NotImplementedException();
        }

        public void RecipePredicted(RecipePrediction recipePrediction)
        {
            throw new NotImplementedException();
        }
    }
}
