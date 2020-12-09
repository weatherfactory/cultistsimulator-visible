using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using UnityEngine.EventSystems;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISphereEventSubscriber
    {
        void OnTokensChangedForSphere(TokenInteractionEventArgs args);
        void OnTokenInteractionInSphere(TokenInteractionEventArgs args);
    }
}
