using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
namespace Assets.CS.TabletopUI
{
    public class SituationWindow : MonoBehaviour,ITokenSubscriber {

        [SerializeField] CanvasGroupFader canvasGroupFader;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Transform cardHolder;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI description;
        [SerializeField] LayoutGroup slotsHolder;
        [SerializeField] AspectsDisplay aspectsDisplay;
        [SerializeField] Button button;
        private Situation situation;

        private Verb verb;
        private List<ISituationWindowSubscriber> subscribers=new List<ISituationWindowSubscriber>();

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

            verb = Registry.Compendium.GetVerbById(box.verbId);

            title.text = verb.Label;
            description.text = verb.Description; 

            //linkedBox.SetSelected(true);
            linkedBox.detailsWindow = this; // this is a bit hacky. We're saving the window in the card so we don't double-open windows.
            // could also track the open windows in tabletop manager instead and check there.

            UpdateSlots();

            canvasGroupFader.SetAlpha(0f);
            canvasGroupFader.Show();
        }

        public VerbBox GetVerbBox() {
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

                var currentAspects = GetAspectsFromSlottedCards();
                   aspectsDisplay.DisplayAspects(currentAspects);
                Recipe r = Registry.Compendium.GetFirstRecipeForAspectsWithVerb(currentAspects, verb.Id);
                DisplayRecipe(r);
                stack.Subscribe(this);
            }
        }


        private Dictionary<string, int> GetAspectsFromSlottedCards()
        {
            ElementStacksGateway ecg = new ElementStacksGateway(new TabletopElementStacksWrapper(slotsHolder.transform));
            Dictionary<string, int> currentAspects = ecg.GetTotalAspects();
            return currentAspects;
        }

        private void DisplayRecipe(Recipe r)
        {
            if (r != null)
            {
                title.text = r.Label;
                description.text = r.StartDescription;
            }
            else
            {
                title.text = "";
                description.text = "";
            }
        }

        void HandleOnButtonClicked()
        {
            var aspects = GetAspectsFromSlottedCards();
            var recipe = Registry.Compendium.GetFirstRecipeForAspectsWithVerb(aspects, verb.Id);
            linkedBox.BeginSituation(recipe);
            subscribers.ForEach(s => s.SituationBegins(linkedBox));
        }

        public void TokenPickedUp(DraggableToken draggableToken)
        {
            var currentAspects = GetAspectsFromSlottedCards();
            Recipe r = Registry.Compendium.GetFirstRecipeForAspectsWithVerb(currentAspects, verb.Id);
            DisplayRecipe(r);
            draggableToken.Unsubscribe(this);
        }

        public void TokenInteracted(DraggableToken draggableToken)
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(ISituationWindowSubscriber subscriber)
        {
            if(!subscribers.Contains((subscriber)))
                subscribers.Add(subscriber);
        }
    }
}
