using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ElementCardClickable : ElementCard, IPointerClickHandler {
	public event System.Action<ElementCard> onCardClicked;

	public void OnPointerClick(PointerEventData eventData) {
		if (onCardClicked != null)
			onCardClicked( this );
	}
}
