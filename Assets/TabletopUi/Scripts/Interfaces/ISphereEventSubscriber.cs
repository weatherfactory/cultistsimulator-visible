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
        void NotifyTokensChangedForSphere(TokenEventArgs args);
        void OnTokenClicked(TokenEventArgs args);
        void OnTokenReceivedADrop(TokenEventArgs args);
        void OnTokenPointerEntered(TokenEventArgs args);
        void OnTokenPointerExited(TokenEventArgs args);
        void OnTokenDoubleClicked(TokenEventArgs args);
        void OnTokenDragged(TokenEventArgs args);
    }
}
