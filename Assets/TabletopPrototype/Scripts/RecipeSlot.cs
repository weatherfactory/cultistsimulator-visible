using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class RecipeSlot : MonoBehaviour, IDropHandler {

	public event System.Action<RecipeSlot> onCardDropped;

	// TODO: Needs hover feedback!

	public void OnDrop(PointerEventData eventData) {
		if (onCardDropped != null)
			onCardDropped(this);
	}
}
