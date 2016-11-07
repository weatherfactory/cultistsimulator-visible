using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TabletopManager : MonoBehaviour {

	[SerializeField] Transform cardHolder;
	[SerializeField] Transform windowParent;
	[SerializeField] ElementCardClickable elementCardPrefab;
	[SerializeField] ElementDetailsWindow elementDetailWindowPrefab;

	private Compendium compendium;

	// Use this for initialization
	void Start () {
		compendium = new Compendium(new Dice());
		ContentImporter ContentImporter = new ContentImporter();
		ContentImporter.PopulateCompendium(compendium);
		
		BuildCards(10);
	}

	void BuildCards(int numCards) {
		for (int i = 0; i < numCards; i++) {
			var card = BuildCard();
			card.SetElement("test" + i, 1, compendium);
			card.transform.localPosition = new Vector3(-1000f + i * 220f, 0f);
		}
	}

	// Ideally we pool and reuse these
	ElementCardClickable BuildCard() {
		var card = Instantiate(elementCardPrefab) as ElementCardClickable;
		card.onCardClicked += HandleOnElementCardClicked;
		card.transform.SetParent(cardHolder);
		card.transform.localScale = Vector3.one;
		card.transform.localPosition = Vector3.zero;
		return card;
	}

	void HandleOnElementCardClicked(ElementCard card) {
		ShowElementDetails(card);
	}

	void ShowElementDetails(ElementCard card) {
		var window = BuildElementDetailsWindow();
		window.transform.position = card.transform.position;
		window.SetElementCard(card);
	}

	// Ideally we pool and reuse these
	ElementDetailsWindow BuildElementDetailsWindow() {
		var card = Instantiate(elementDetailWindowPrefab) as ElementDetailsWindow;
		card.transform.SetParent(windowParent);
		card.transform.localScale = Vector3.one;
		return card;
	}
}
