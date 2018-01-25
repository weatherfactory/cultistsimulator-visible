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

            Debug.Log("OnTableDrop: Rect Pos " + DraggableToken.itemBeingDragged.RectTransform.rect + " on parent " + DraggableToken.itemBeingDragged.transform.parent);
            Debug.Log("OnTableDrop: Pointer Pos " + eventData.position);
            

            if (onDropped != null)
                onDropped();
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (onClicked != null)
                onClicked();
        }
    }
}
