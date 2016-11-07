using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class ElementDetailsWindow : MonoBehaviour {

	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] Transform cardHolder;
	[SerializeField] TextMeshProUGUI name;
	[SerializeField] TextMeshProUGUI description;
	[SerializeField] TextMeshProUGUI slots;
	[SerializeField] TextMeshProUGUI aspects;

	ElementCard linkedCard;

	// Make sure we subscribe to draggable changes to know when our linkedCard is being dragged
	void OnEnable () {
		Draggable.onChangeDragState += OnChangeDragState;
	}
	// Make sure we unsubscribe when this gets disabled or destroyed to avoid further calls
	void OnDisable() {
		Draggable.onChangeDragState -= OnChangeDragState;
	}

	void OnChangeDragState (bool isDragging) {
		// We're dragging the card that this window is linked to
		if (isDragging && Draggable.itemBeingDragged.gameObject == linkedCard.gameObject)
			Hide();
	}

	public void SetElementCard(ElementCard card) {
		linkedCard = card;

		card.transform.SetParent(cardHolder);
		card.transform.localPosition = Vector3.zero;
		card.transform.localRotation = Quaternion.identity;

		// This data needs to come from the Compendium, but it's currently not accessible here
		name.text = card.name;
		description.text = "Test Description for "+card.elementId; 
		slots.text = "Test Slots for "+card.elementId; 
		aspects.text = "Test Aspects for "+card.elementId;
	}

	public void Hide() {
		GameObject.Destroy(gameObject);
	}

}
