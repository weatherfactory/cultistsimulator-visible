using System.Collections.Generic;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
namespace Assets.CS.TabletopUI
{
    public class SituationWindow : MonoBehaviour {

        // This event should probably either give the window, which contains a reference to the recipe
        // or it gives out the recipe directly
        public event System.Action<SituationWindow, VerbBox> onStartRecipe;

        [SerializeField] CanvasGroupFader canvasGroupFader;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Transform cardHolder;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI description;
        [SerializeField] LayoutGroup slotsHolder;
        [SerializeField] AspectsDisplay aspectsDisplay;
        [SerializeField] Button button;


        private Verb verb;

        VerbBox linkedBox;
        List<RecipeSlot> slots = new List<RecipeSlot>();


        void OnEnable () {
            button.onClick.AddListener(HandleOnButtonClicked);
        }
        void OnDisable() {
            button.onClick.RemoveListener(HandleOnButtonClicked);
        }


        // TODO: This should check the state of the verbBox/recipe and display the box accordingly.
        public void SetVerb(VerbBox box) {
            linkedBox = box;

            box.transform.SetParent(cardHolder); // We probably shouldn't reparent here, this makes things a bit iffy. 
            // Instead we should lock positions in some other way?
            // Window subscribes to verb/element, and when it's position is changed window updates its own?
            box.transform.localPosition = Vector3.zero;
            box.transform.localRotation = Quaternion.identity;

            verb = Registry.compendium.GetVerbById(box.verbId);

            title.text = verb.Label;
            description.text = verb.Description; 

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
            var slot =PrefabFactory.CreateLocally<RecipeSlot>(slotsHolder.transform);
            slot.onCardDropped += HandleOnSlotDroppedOn;
            return slot;
        }

        public void Hide() {
            //linkedBox.SetSelected(false);
            linkedBox.detailsWindow = null; // this is hacky. We're saving the window in the card so we don't double-open windows.
            canvasGroupFader.destroyOnHide = true;
            canvasGroupFader.Hide();
        }

        public ElementStack[] GetAllHeldCards() {
            return slotsHolder.GetComponentsInChildren<ElementStack>();
        }


        void HandleOnSlotDroppedOn(RecipeSlot slot) {
            // should this be sent through to the tabletop manager?
            Debug.Log("Recipe Slot dropped on");

            ElementStack stack=DraggableToken.itemBeingDragged as ElementStack;
            if(stack!=null)
            { 
                DraggableToken.resetToStartPos = false; // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
                stack.transform.SetParent(slot.transform); // Make sure to parent back to the tabletop
                stack.transform.localPosition = Vector3.zero;
                stack.transform.localRotation = Quaternion.identity;
                ElementStacksGateway ecg=new ElementStacksGateway(GetAllHeldCards(),null);
                Dictionary<string,int> currentAspects = ecg.GetTotalAspects();
                aspectsDisplay.DisplayAspects(currentAspects);
                Recipe r = Registry.compendium.GetFirstRecipeForAspectsWithVerb(currentAspects, verb.Id);
                if(r!=null)
                {
                    title.text = r.Label;
                    description.text = r.StartDescription;
                }
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
