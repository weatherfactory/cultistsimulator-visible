using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TabletopBackground : MonoBehaviour, IDropHandler {

	public event System.Action onDropped;

	public void OnDrop(PointerEventData eventData) {
		if (onDropped != null)
			onDropped();
	}
}
