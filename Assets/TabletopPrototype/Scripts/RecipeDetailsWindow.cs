using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
namespace Assets.CS.TabletopUI
{
    public class RecipeDetailsWindow : MonoBehaviour {

        // This event should probably either give the window, which contains a reference to the recipe
        // or it gives out the recipe directly
        public event System.Action<RecipeDetailsWindow, VerbBox> onStartRecipe;

        [SerializeField] CanvasGroupFader canvasGroupFader;
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

            box.transform.SetParent(cardHolder); // We probably shouldn't reparent here, this makes things a bit iffy. 
            // Instead we should lock positions in some other way?
            // Window subscribes to verb/element, and when it's position is changed window updates it's own?
            box.transform.localPosition = Vector3.zero;
            box.transform.localRotation = Quaternion.identity;

            var verb = CompendiumHolder.compendium.GetVerbById(box.verbId);

            title.text = verb.Label;
            description.text = verb.Description; 
            aspects.text = "Aspects go here...";

            //linkedBox.SetSelected(true);
            linkedBox.detailsWindow = this; // this is a bit hacky. We're saving the window in the card so we don't double-open windows.
            // could also track the open windows in tabletop manager instead and check there.

            UpdateSlots();

            canvasGroupFader.SetAlpha(0f);
            canvasGroupFader.Show();
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
            //linkedBox.SetSelected(false);
            linkedBox.detailsWindow = null; // this is hacky. We're saving the window in the card so we don't double-open windows.
            canvasGroupFader.destroyOnHide = true;
            canvasGroupFader.Hide();
        }

        public ElementCard[] GetAllHeldCards() {
            return slotsHolder.GetComponentsInChildren<ElementCard>();
        }


        void HandleOnSlotDroppedOn(RecipeSlot slot) {
            // This should be given through to the tabletop manager, normally.
            Debug.Log("Recipe Slot dropped on");

            if (DraggableToken.itemBeingDragged != null && DraggableToken.itemBeingDragged.GetComponent<ElementCard>() != null) { // Maybe check for item type here via GetComponent<Something>() != null?
                DraggableToken.resetToStartPos = false; // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
                DraggableToken.itemBeingDragged.transform.SetParent(slot.transform); // Make sure to parent back to the tabletop
                DraggableToken.itemBeingDragged.transform.localPosition = Vector3.zero;
                DraggableToken.itemBeingDragged.transform.localRotation = Quaternion.identity;
            }
        }

        void HandleOnButtonClicked() {
            // This should be given through to the tabletop manager, normally.
            Debug.Log("Button clicked!");

            if (onStartRecipe != null)
                onStartRecipe(this, linkedBox);
        }

    }
}
