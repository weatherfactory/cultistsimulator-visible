using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class ElementDetailsWindow : MonoBehaviour {

	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] Transform cardHolder;
	[SerializeField] TextMeshProUGUI title;
	[SerializeField] TextMeshProUGUI description;
	[SerializeField] TextMeshProUGUI slots;
	[SerializeField] TextMeshProUGUI aspects;

	ElementCard linkedCard;

	public void SetElementCard(ElementCard card) {
		linkedCard = card;

		card.transform.SetParent(cardHolder);
		card.transform.localPosition = Vector3.zero;
		card.transform.localRotation = Quaternion.identity;

		// This data needs to come from the Compendium, but it's currently not accessible here
		title.text = card.name;
		description.text = "Test Description for "+card.elementId; 
		slots.text = "Test Slots for "+card.elementId; 
		aspects.text = "Test Aspects for "+card.elementId;

		linkedCard.detailsWindow = this; // this is hacky. We're saving the window in the card so we don't double-open windows.
	}

	public ElementCard GetElementCard() {
		return linkedCard;
	}

	public void Hide() {
		linkedCard.detailsWindow = null; // this is hacky. We're saving the window in the card so we don't double-open windows.
		GameObject.Destroy(gameObject);
	}

}
