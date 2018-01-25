using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.CS.TabletopUI
{
    public class TabletopBackground : MonoBehaviour, IDropHandler, IPointerClickHandler {

        public event System.Action onDropped;
        public event System.Action onClicked;

        public void OnDrop(PointerEventData eventData) {
            if (DraggableToken.itemBeingDragged == null)
                return;

            if (onDropped != null)
                onDropped();
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (onClicked != null)
                onClicked();
        }
    }
}
