using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TabletopBackground : MonoBehaviour, IDropHandler {

	public void OnDrop(PointerEventData eventData) {
		if (Draggable.itemBeingDragged != null) { // Maybe check for item type here via GetComponent<Something>() != null?
			Draggable.resetToStartPos = false; // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
			Draggable.itemBeingDragged.transform.SetParent(transform.parent); // Make sure to parent back to the tabletop
		}
	}
}
