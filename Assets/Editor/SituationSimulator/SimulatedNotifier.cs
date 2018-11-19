using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;

namespace Assets.Editor
{
    public class SimulatedNotifier : INotifier
    {
        public void PushTextToLog(string text)
        {
        }

        public void HideDetails()
        {
        }

        public void ShowNotificationWindow(string title, string description)
        {
        }

        public void ShowCardElementDetails(Element element, ElementStackToken token)
        {
        }

        public void ShowElementDetails(Element element, bool fromDetailsWindow = false)
        {
        }

        public void ShowSlotDetails(SlotSpecification slot, bool highlightGreedy, bool highlightConsumes)
        {
        }

        public void ShowDeckDetails(IDeckSpec deckId, int deckQuantity)
        {
        }

        public void ShowSaveError(bool @on)
        {
        }

        public void ShowImageBurn(string spriteName, DraggableToken token, float duration, float scale, TabletopImageBurner.ImageLayoutConfig alignment)
        {
        }
    }
}
