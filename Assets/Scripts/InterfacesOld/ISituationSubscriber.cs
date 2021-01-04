using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Interfaces;

namespace SecretHistories.Interfaces
{
    public interface ISituationSubscriber
    {
        void SituationStateChanged(Situation situation);
        void TimerValuesChanged(Situation s);
        void SituationSphereContentsUpdated(Situation s);
        void ReceiveNotification(INotification n);
    }
}
