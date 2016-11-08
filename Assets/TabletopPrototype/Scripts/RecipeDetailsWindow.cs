using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
public class RecipeDetailsWindow : MonoBehaviour {

	// This event should probably either give the window, which contains a reference to the recipe
	// or it gives out the recipe directly
	public event System.Action<RecipeDetailsWindow, VerbBox> onStartRecipe;

	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] Transform cardHolder;
	[SerializeField] TextMeshProUGUI title;
	[SerializeField] TextMeshProUGUI description;
	[SerializeField] LayoutGroup slotsHolder;
	[SerializeField] TextMeshProUGUI aspects;
	[SerializeField] RecipeSlot slotPrefab;
	[SerializeField] Button button;

	VerbBox linkedBox;
	List<RecipeSlot> slots = new List<RecipeSlot>();

	// Make sure we subscribe to draggable changes to know when our linkedCard is being dragged
	void OnEnable () {
		button.onClick.AddListener(HandleOnButtonClicked);
	}
	// Make sure we unsubscribe when this gets disabled or destroyed to avoid further
	void OnDisable() {
		button.onClick.RemoveListener(HandleOnButtonClicked);
	}


	// TODO: This should check the state of the verbBox/recipe and display the box accordingly.
	public void SetVerb(VerbBox box) {
		linkedBox = box;

		box.transform.SetParent(cardHolder);
		box.transform.localPosition = Vector3.zero;
		box.transform.localRotation = Quaternion.identity;

		// This data needs to come from the Compendium, but it's currently not accessible here
		// We don't have a reference to it and honestly, it's going to be used in every one of these display objects
		// setting references seems unecessary, but then again I'm not that opposed to statics and singletons as you are ;)
		title.text = box.name;
		description.text = "Test Description for "+box.verbId; 
		aspects.text = "Test Aspects for "+box.verbId;

		linkedBox.detailsWindow = this; // this is a bit hacky. We're saving the window in the card so we don't double-open windows.
		// could also track the open windows in tabletop manager instead and check there.

		UpdateSlots();
	}

	public VerbBox GetVerb() {
		return linkedBox;
	}

	public void UpdateSlots() {		
		int numSlots = 3;

		for (int i = 0; i < numSlots; i++) {
			if (i >= slots.Count)
				slots.Add( BuildSlot() );
		}
	}

	RecipeSlot BuildSlot() {
		var slot = Instantiate(slotPrefab) as RecipeSlot;
		slot.onCardDropped += HandleOnSlotDroppedOn;
		slot.transform.SetParent(slotsHolder.transform);
		slot.transform.localScale = Vector3.one;
		slot.transform.localPosition = Vector3.zero;
		slot.transform.localRotation = Quaternion.identity;
		return slot;
	}

	public void Hide() {
		linkedBox.detailsWindow = null; // this is hacky. We're saving the window in the card so we don't double-open windows.
		GameObject.Destroy(gameObject);
	}

	public ElementCard[] GetAllHeldCards() {
		return slotsHolder.GetComponentsInChildren<ElementCard>();
	}


	void HandleOnSlotDroppedOn(RecipeSlot slot) {
		// This should be given through to the tabletop manager, normally.
		Debug.Log("Recipe Slot dropped on");

		if (Draggable.itemBeingDragged != null && Draggable.itemBeingDragged.GetComponent<ElementCard>() != null) { // Maybe check for item type here via GetComponent<Something>() != null?
			Draggable.resetToStartPos = false; // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
			Draggable.itemBeingDragged.transform.SetParent(slot.transform); // Make sure to parent back to the tabletop
			Draggable.itemBeingDragged.transform.localPosition = Vector3.zero;
			Draggable.itemBeingDragged.transform.localRotation = Quaternion.identity;
		}
	}

	void HandleOnButtonClicked() {
		// This should be given through to the tabletop manager, normally.
		Debug.Log("Button clicked!");

		if (onStartRecipe != null)
			onStartRecipe(this, linkedBox);
	}

}
