﻿using System;
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
        void SituationStateChanged(Situation situation);
        void TimerValuesChanged(Situation s);
        void SphereContentsUpdated(Situation s);
        void ReceiveNotification(INotification n);
    }
}