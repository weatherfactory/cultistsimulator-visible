using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TabletopBackground : MonoBehaviour, IDropHandler, IPointerClickHandler {

	public event System.Action onDropped;
	public event System.Action onClicked;

	public void OnDrop(PointerEventData eventData) {
		if (onDropped != null)
			onDropped();
	}

	public void OnPointerClick(PointerEventData eventData) {
		if (onClicked != null)
			onClicked();
	}
}
