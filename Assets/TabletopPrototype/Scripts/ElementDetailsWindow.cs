using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
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

		var element = CompendiumHolder.compendium.GetElementById(card.elementId);

		title.text = element.Label;
		description.text = element.Description; 
		slots.text = "Slots: "+element.ChildSlotSpecifications.Count; 
		aspects.text = "Aspects: "+GetAspects(element.Aspects);

		linkedCard.detailsWindow = this; // this is hacky. We're saving the window in the card so we don't double-open windows.
	}

	string GetAspects(Dictionary<string, int> aspects) {
		if (aspects == null || aspects.Count == 0)
			return "None.";

		var stringBuilder = new System.Text.StringBuilder();
		int i = 0;

		foreach (var keyValuePair in aspects) {
			stringBuilder.Append(keyValuePair.Key);
			stringBuilder.Append(" ");
			stringBuilder.Append(keyValuePair.Value);
			i++;

			if (i < aspects.Count)
				stringBuilder.Append(", ");
			else 
				stringBuilder.Append(".");
		}

		return stringBuilder.ToString();
	}

	public ElementCard GetElementCard() {
		return linkedCard;
	}

	public void Hide() {
		linkedCard.detailsWindow = null; // this is hacky. We're saving the window in the card so we don't double-open windows.
		GameObject.Destroy(gameObject);
	}

}
