using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class RecipeDetailsWindow : MonoBehaviour {

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
		Draggable.onChangeDragState += OnChangeDragState;
		button.onClick.AddListener(HandleOnButtonClicked);
	}
	// Make sure we unsubscribe when this gets disabled or destroyed to avoid further
	void OnDisable() {
		Draggable.onChangeDragState -= OnChangeDragState;
		button.onClick.RemoveListener(HandleOnButtonClicked);
	}

	void OnChangeDragState (bool isDragging) {
		// We're dragging the card that this window is linked to
		if (isDragging && Draggable.itemBeingDragged.gameObject == linkedBox.gameObject)
			Hide();
	}

	public void SetVerb(VerbBox box) {
		linkedBox = box;

		box.transform.SetParent(cardHolder);
		box.transform.localPosition = Vector3.zero;
		box.transform.localRotation = Quaternion.identity;

		// This data needs to come from the Compendium, but it's currently not accessible here
		title.text = box.name;
		description.text = "Test Description for "+box.verbId; 
		aspects.text = "Test Aspects for "+box.verbId;

		linkedBox.detailsWindow = this; // this is hacky. We're saving the window in the card so we don't double-open windows.

		UpdateSlots();
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
	}

}
