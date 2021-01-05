﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Infrastructure.Events;
using UnityEngine.EventSystems;

namespace SecretHistories.Interfaces
{
    public interface ISphereEventSubscriber
    {
        void OnTokensChangedForSphere(TokenInteractionEventArgs args);
        void OnTokenInteractionInSphere(TokenInteractionEventArgs args);
    }
}