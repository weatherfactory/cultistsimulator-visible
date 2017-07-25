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
        void DebugLog(string text);
        void PushTextToLog(string text);
        void ShowNotificationWindow(string title, string description, float duration = 10);
        void ShowElementDetails(Element element);
        void ShowSlotDetails(SlotSpecification slot);
        void TokenReturnedToTabletop(DraggableToken draggableToken, INotification reason);
        void ShowImageBurn(string spriteName, DraggableToken token, float duration, float scale, TabletopImageBurner.ImageLayoutConfig alignment);
    }
}
