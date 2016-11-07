using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This is a "version" of the discussed BoardManager. Creates View Objects, Listens to their input.
public class TabletopManager : MonoBehaviour {

	[Header("Existing Objects")]
	[SerializeField] Transform cardHolder;
	[SerializeField] Transform windowParent;
	[SerializeField] TabletopBackground background;

	[Header("Prefabs")]
	[SerializeField] ElementCardClickable elementCardPrefab;
	[SerializeField] ElementDetailsWindow elementDetailWindowPrefab;
	[SerializeField] VerbBox verbBoxPrefab;
	[SerializeField] RecipeDetailsWindow recipeDetailsWindowPrefab;

	private Compendium compendium;

	void Start () {
		compendium = new Compendium(new Dice());
		ContentImporter ContentImporter = new ContentImporter();
		ContentImporter.PopulateCompendium(compendium);

		// Init Listeners to pre-existing Display Objects
		background.onDropped += HandleOnBackgroundDropped;

		// Build initial test data
		BuildTestData();
	}

	void BuildTestData() {

		VerbBox box;
		ElementCardClickable card;

		// build verbs
		var verbs = compendium.GetAllVerbs();

		for (int i = 0; i < verbs.Count; i++) {
			box = BuildVerbBox();
			box.SetVerb(verbs[i]);
			box.transform.localPosition = new Vector3(-1000f + i * 320f, 400f);
		}

		// 10 test cards
		for (int i = 0; i < 10; i++) {
			card = BuildElementCard();
			card.SetElement(legalElementIDs[i % legalElementIDs.Length], 1, compendium);
			card.transform.localPosition = new Vector3(-1000f + i * 220f, 0f);
		}
	}

	string[] legalElementIDs = new string[7] { 
		"health",  
		"reason",  
		"clique",  
		"ordinarylife",
		"suitablepremises",
		"occultscrap",
		"shilling"
	};

	#region -- CREATE VIEW OBJECTS ----------------------------------------------------

	// Verb Boxes

	// Ideally we pool and reuse these
	VerbBox BuildVerbBox() {
		var box = Instantiate(verbBoxPrefab) as VerbBox;
		box.onVerbBoxClicked += HandleOnVerbBoxClicked;
		box.transform.SetParent(cardHolder);
		box.transform.localScale = Vector3.one;
		box.transform.localPosition = Vector3.zero;
		return box;
	}

	// Element Cards

	// Ideally we pool and reuse these
	ElementCardClickable BuildElementCard() {
		var card = Instantiate(elementCardPrefab) as ElementCardClickable;
		card.onCardClicked += HandleOnElementCardClicked;
		card.transform.SetParent(cardHolder);
		card.transform.localScale = Vector3.one;
		card.transform.localPosition = Vector3.zero;
		return card;
	}

	// Element Detail Windows

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

	// Recipe Detail Windows

	void ShowRecipeDetails(VerbBox card) {
		var window = BuildRecipeDetailsWindow();
		window.transform.position = card.transform.position;
		window.SetVerb(card);
	}

	// Ideally we pool and reuse these
	RecipeDetailsWindow BuildRecipeDetailsWindow() {
		var card = Instantiate(recipeDetailsWindowPrefab) as RecipeDetailsWindow;
		card.transform.SetParent(windowParent);
		card.transform.localScale = Vector3.one;
		return card;
	}

	#endregion

	#region -- INTERACTION ----------------------------------------------------

	void HandleOnBackgroundDropped() {
		if (Draggable.itemBeingDragged != null) { // Maybe check for item type here via GetComponent<Something>() != null?
			Draggable.resetToStartPos = false; // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
			Draggable.itemBeingDragged.transform.SetParent(cardHolder); // Make sure to parent back to the tabletop
		}
	}

	void HandleOnElementCardClicked(ElementCard card) {
		if (card.detailsWindow == null)
			ShowElementDetails(card);
		else {
			card.transform.SetParent(cardHolder); // remove card from details window before hiding it, so it isn't removed
			card.detailsWindow.Hide();
		}
	}

	void HandleOnVerbBoxClicked(VerbBox box) {
		if (box.detailsWindow == null)
			ShowRecipeDetails(box);
		else {
			box.transform.SetParent(cardHolder); // remove verb from details window before hiding it, so it isn't removed

			var heldCards = box.detailsWindow.GetAllHeldCards();

			foreach (var item in heldCards) 
				item.transform.SetParent(cardHolder); // remove cards from details window before hiding it, so they aren't removed

			box.detailsWindow.Hide();
		}
	}

	#endregion
}
