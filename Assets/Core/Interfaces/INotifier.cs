using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.Core.Interfaces
{
    public interface INotifier
    {
        void PushTextToLog(string text);
        void ShowNotificationWindow(string title, string description, float duration = 10);
        void ShowCardElementDetails(Element element, ElementStackToken token);
        void ShowElementDetails(Element element, bool fromDetailsWindow = false);
        void ShowSlotDetails(SlotSpecification slot);
        void ShowTokenReturnToTabletopNotification(DraggableToken draggableToken, INotification reason);
        void ShowImageBurn(string spriteName, DraggableToken token, float duration, float scale, TabletopImageBurner.ImageLayoutConfig alignment);
    }
}
