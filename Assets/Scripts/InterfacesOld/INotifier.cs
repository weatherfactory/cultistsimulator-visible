using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.UI;
using SecretHistories.Interfaces;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.Services;

namespace SecretHistories.Interfaces
{
    public interface INotifier:ISphereCatalogueEventSubscriber
    {
        void PushTextToLog(string text);
        void HideDetails();
        void ShowNotificationWindow(NotificationArgs args);
        void ShowNotificationWindow(string title,string description, bool duplicatesAllowed);
        void ShowCardElementDetails(Element element, ElementStack token);
        void ShowElementDetails(Element element, bool fromDetailsWindow = false);
        void ShowSlotDetails(SlotSpecification slot, bool highlightGreedy, bool highlightConsumes);
        void ShowDeckDetails(DeckSpec deckId, int deckQuantity);
        void ShowImageBurn(string spriteName, Token token, float duration, float scale, TabletopImageBurner.ImageLayoutConfig alignment);
    }
}
