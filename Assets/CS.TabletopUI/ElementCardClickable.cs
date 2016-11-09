using UnityEngine.EventSystems;

namespace Assets.CS.TabletopUI
{
    public class ElementCardClickable : ElementCard, IPointerClickHandler {
        public event System.Action<ElementCard> onCardClicked;

        public void OnPointerClick(PointerEventData eventData) {
            // pointerID n-0 are touches, -1 is LMB. This prevents drag from RMB, MMB and other mouse buttons (-2, -3...)
            if ( eventData.pointerId >= -1 && onCardClicked != null)
                onCardClicked( this );
        }
    }
}
