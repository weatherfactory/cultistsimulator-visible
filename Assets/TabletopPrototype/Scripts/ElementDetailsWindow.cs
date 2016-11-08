using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ElementDetailsWindow : MonoBehaviour {

	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] Transform cardHolder;
	[SerializeField] Image artwork;
	[SerializeField] TextMeshProUGUI title;
	[SerializeField] TextMeshProUGUI description;
	[SerializeField] TextMeshProUGUI slots;
	[SerializeField] TextMeshProUGUI aspects;

	ElementCard linkedCard;

	public void SetElementCard(ElementCard card) {
		linkedCard = card;

//		card.transform.SetParent(cardHolder);
//		card.transform.localPosition = Vector3.zero;
//		card.transform.localRotation = Quaternion.identity;

		// This data needs to come from the Compendium, but it's currently not accessible here

		var element = CompendiumHolder.compendium.GetElementById(card.elementId);

		artwork.sprite = card.GetSprite();
		title.text = element.Label;
		description.text = element.Description; 
		slots.text = GetSlotsText(element.ChildSlotSpecifications); 
		aspects.text = "Aspects: "+GetAspectsText(element.Aspects);

		linkedCard.SetSelected(true);
		linkedCard.detailsWindow = this; // this is hacky. We're saving the window in the card so we don't double-open windows.
	}

	string GetSlotsText(List<ChildSlotSpecification> slots) { // THis could be in a TOString methodto be more accessible where it's needed?
		if (slots == null || slots.Count == 0)
			return "Slots: None";

		var stringBuilder = new System.Text.StringBuilder("Slots: "+slots.Count +"\n");

		for (int i = 0; i < slots.Count; i++) {
			stringBuilder.Append(slots[i].Label);

			if (slots[i].Required.Count > 0 || slots[i].Forbidden.Count > 0)
				stringBuilder.Append(" (");

			if (slots[i].Required.Count > 0) {
				stringBuilder.Append("Required: ");
				stringBuilder.Append(GetAspectsText(slots[i].Required));
			}

			if (slots[i].Required.Count > 0 && slots[i].Forbidden.Count > 0)
				stringBuilder.Append(" | ");
			
			if (slots[i].Forbidden.Count > 0) {
				stringBuilder.Append("Forbidden: ");
				stringBuilder.Append(GetAspectsText(slots[i].Forbidden));
			}

			if (slots[i].Required.Count > 0 || slots[i].Forbidden.Count > 0)
				stringBuilder.Append(")");

			if (i + 1 < slots.Count)
				stringBuilder.Append("\n");
		}

		return stringBuilder.ToString();
	}

	string GetAspectsText(Dictionary<string, int> aspects) {// THis could be in a TOString method to be more accessible where it's needed?
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
		}

		return stringBuilder.ToString();
	}

	public ElementCard GetElementCard() {
		return linkedCard;
	}

	public void Hide() {
		linkedCard.SetSelected(false);
		linkedCard.detailsWindow = null; // this is hacky. We're saving the window in the card so we don't double-open windows.
		GameObject.Destroy(gameObject);
	}

}
