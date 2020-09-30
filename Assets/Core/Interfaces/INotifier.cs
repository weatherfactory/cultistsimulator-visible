using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Core.Entities;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;

namespace Assets.Core.Interfaces
{
    public interface INotifier:ITokenObserver
    {
        void PushTextToLog(string text);
        void HideDetails();
        void ShowNotificationWindow(NotificationArgs args);
        void ShowNotificationWindow(string title,string description, bool duplicatesAllowed);
        void ShowCardElementDetails(Element element, ElementStackToken token);
        void ShowElementDetails(Element element, bool fromDetailsWindow = false);
        void ShowSlotDetails(SlotSpecification slot, bool highlightGreedy, bool highlightConsumes);
        void ShowDeckDetails(IDeckSpec deckId, int deckQuantity);
        void ShowSaveError(bool on);
        void ShowImageBurn(string spriteName, DraggableToken token, float duration, float scale, TabletopImageBurner.ImageLayoutConfig alignment);
    }
}
