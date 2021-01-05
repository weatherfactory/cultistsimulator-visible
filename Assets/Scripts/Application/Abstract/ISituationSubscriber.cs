﻿
using SecretHistories.Entities;

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