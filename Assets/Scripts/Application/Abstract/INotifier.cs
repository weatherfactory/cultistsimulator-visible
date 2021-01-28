using SecretHistories.UI;
using SecretHistories.Entities;
using SecretHistories.Services;

namespace SecretHistories.Interfaces
{
    public interface INotifier
    {
        void PushTextToLog(string text);
        void HideDetails();
        void ShowNotificationWindow(NotificationArgs args);
        void ShowNotificationWindow(string title,string description, bool duplicatesAllowed);
        void ShowCardElementDetails(Element element, ElementStack stack);
        void ShowElementDetails(Element element, bool fromDetailsWindow = false);
        void ShowSlotDetails(SphereSpec slot, bool highlightGreedy, bool highlightConsumes);
        void ShowDeckDetails(DeckSpec deckId, int deckQuantity);
        void ShowImageBurn(string spriteName, Token token, float duration, float scale, TabletopImageBurner.ImageLayoutConfig alignment);
    }
}
