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

	[Header("View Settings")]
	[SerializeField] float windowZOffset = -10f;

	private Compendium compendium;

	void Start () {
		compendium = new Compendium(new Dice());
		ContentImporter ContentImporter = new ContentImporter();
		ContentImporter.PopulateCompendium(compendium);

		// Init Listeners to pre-existing Display Objects
		background.onDropped += HandleOnBackgroundDropped;
		Draggable.onChangeDragState += OnChangeDragState;

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

	#region -- CREATE / REMOVE VIEW OBJECTS ----------------------------------------------------

	// Verb Boxes

	// Ideally we pool and reuse these
	VerbBox BuildVerbBox() {
		var box = Instantiate(verbBoxPrefab) as VerbBox;
		box.onVerbBoxClicked += HandleOnVerbBoxClicked;
		box.transform.SetParent(cardHolder);
		box.transform.localScale = Vector3.one;
		box.transform.localPosition = Vector3.zero;
		box.transform.localRotation = Quaternion.identity;
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
		card.transform.localRotation = Quaternion.identity;
		return card;
	}

	// Element Detail Windows

	void ShowElementDetails(ElementCard card) {
		if (maxNumElementWindows > 0 && elementWindows.Count == maxNumElementWindows) 
			HideElementDetails(elementWindows[0].GetElementCard());

		PutTokenInAir(card.transform as RectTransform);

		var window = BuildElementDetailsWindow();
		window.transform.position = card.transform.position;
		window.SetElementCard(card);
		elementWindows.Add(window);
	}

	void HideElementDetails(ElementCard card) {
		PutTokenOnTable(card.transform as RectTransform); // remove card from details window before hiding it, so it isn't removed
		elementWindows.Remove(card.detailsWindow);
		card.detailsWindow.Hide();
	}

	// Ideally we pool and reuse these
	ElementDetailsWindow BuildElementDetailsWindow() {
		var window = Instantiate(elementDetailWindowPrefab) as ElementDetailsWindow;
		window.transform.SetParent(windowParent);
		window.transform.localScale = Vector3.one;
		window.transform.localRotation = Quaternion.identity;
		return window;
	}

	// Recipe Detail Windows

	void ShowRecipeDetails(VerbBox box) {
		if (maxNumRecipeWindows > 0 && recipeWindows.Count == maxNumRecipeWindows) 
			HideRecipeDetails(recipeWindows[0].GetVerb(), true);

		PutTokenInAir(box.transform as RectTransform);

		var window = BuildRecipeDetailsWindow();
		window.transform.position = box.transform.position;
		window.SetVerb(box);
		recipeWindows.Add(window);
	}

	void HideRecipeDetails(VerbBox box, bool keepCards) {
		PutTokenOnTable(box.transform as RectTransform); // remove verb from details window before hiding it, so it isn't removed

		if (keepCards) {
			var heldCards = box.detailsWindow.GetAllHeldCards();

			foreach (var item in heldCards)
				PutTokenOnTable(item.transform as RectTransform); // remove cards from details window before hiding it, so they aren't removed
		}

		recipeWindows.Remove(box.detailsWindow);
		box.detailsWindow.Hide();
	}

	// Ideally we pool and reuse these
	RecipeDetailsWindow BuildRecipeDetailsWindow() {
		var window = Instantiate(recipeDetailsWindowPrefab) as RecipeDetailsWindow;
		window.transform.SetParent(windowParent);
		window.transform.localScale = Vector3.one;
		window.transform.localRotation = Quaternion.identity;
		window.onStartRecipe += HandleOnRecipeStarted;
		return window;
	}

	public int maxNumRecipeWindows = 1;
	public int maxNumElementWindows = 1;
	List<RecipeDetailsWindow> recipeWindows = new List<RecipeDetailsWindow>();
	List<ElementDetailsWindow> elementWindows = new List<ElementDetailsWindow>();

	#endregion

	#region -- MOVE / CHANGE VIEW OBJECTS ----------------------------------------------------

	// parents object to "CardHolder" (should rename to TokenHolder) and sets it's Z to 0.
	public void PutTokenOnTable(RectTransform rectTransform) {
		if (rectTransform == null)
			return;

		rectTransform.SetParent(cardHolder); 
		rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0f);
	}

	// parents object to "CardHolder" (should rename to TokenHolder) and sets it's Z to 0.
	public void PutTokenInAir(RectTransform rectTransform) {
		if (rectTransform == null)
			return;

		rectTransform.SetParent(cardHolder); 
		rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, windowZOffset);
	}


	#endregion

	#region -- INTERACTION ----------------------------------------------------

	// This checks if we're dragging something and if we have to do some changes
	// Currently this closes the windows when dragging is being done
	// This was in the windows previously, but since i'm not holding a reference to 
	// all open windows here it had to move up.
	void OnChangeDragState (bool isDragging) {
		if (isDragging == false || Draggable.itemBeingDragged.gameObject == null)
			return;

		ElementCard card = Draggable.itemBeingDragged.GetComponent<ElementCard>();

		if (card != null) {
			if (card.detailsWindow != null)
				HideElementDetails(card);
			else
				return;			
		}

		VerbBox box = Draggable.itemBeingDragged.GetComponent<VerbBox>();

		if (box != null) {
			if (box.detailsWindow != null)
				HideRecipeDetails(box, true);
			else
				return;			
		}
	}

	void HandleOnBackgroundDropped() {
		// NOTE: This puts items back on the background. We need this in more cases. Should be a method
		if (Draggable.itemBeingDragged != null) { // Maybe check for item type here via GetComponent<Something>() != null?
			Draggable.resetToStartPos = false; // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
			Draggable.itemBeingDragged.transform.SetParent(cardHolder); // Make sure to parent back to the tabletop
			Draggable.itemBeingDragged.transform.localRotation = Quaternion.Euler(0f, 0f, Draggable.itemBeingDragged.transform.eulerAngles.z);
		}
	}

	void HandleOnElementCardClicked(ElementCard card) {
		if (card.detailsWindow == null)
			ShowElementDetails(card);
		else {
			HideElementDetails(card);
		}
	}

	void HandleOnVerbBoxClicked(VerbBox box) {
		if (box.detailsWindow == null)
			ShowRecipeDetails(box);
		else {
			HideRecipeDetails(box, true);
		}
	}

	void HandleOnRecipeStarted(RecipeDetailsWindow window, VerbBox box) {
		HideRecipeDetails(box, false);
		box.StartTimer();
	}

	#endregion
}
