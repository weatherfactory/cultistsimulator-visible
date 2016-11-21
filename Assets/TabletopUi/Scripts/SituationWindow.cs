using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
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
        [SerializeField] Transform slotsHolder;
        [SerializeField] AspectsDisplay aspectsDisplay;
        [SerializeField] Button button;
        [SerializeField] private TextMeshProUGUI NextRecipe;

        private RecipeSlot primarySlot;

       public SituationToken linkedToken;

        void OnEnable () {
            button.onClick.AddListener(HandleOnButtonClicked);
        }
        void OnDisable() {
            button.onClick.RemoveListener(HandleOnButtonClicked);
        }

        public Transform GetSlotsHolder()
        {
            //wrapper so we can change without coupling
            return slotsHolder;
        }

        private void DisplayBusy()
        {
            button.gameObject.SetActive(false);
            NextRecipe.gameObject.SetActive(true);
            ClearAndDestroySlot(primarySlot);
            NextRecipe.text = linkedToken.GetNextRecipeDescription();
        }

        private void DisplayReady()
        {
            button.gameObject.SetActive(true);
            NextRecipe.gameObject.SetActive(false);
            primarySlot = BuildSlot();
            ArrangeSlots();
        }

        public void PopulateAndShow(SituationToken situationToken) {
            linkedToken = situationToken;
            situationToken.transform.SetParent(cardHolder); // We probably shouldn't reparent here, this makes things a bit iffy. 
            // Instead we should lock positions in some other way?
            // Window subscribes to verb/element, and when it's position is changed window updates its own?
            situationToken.transform.localPosition = Vector3.zero;
            situationToken.transform.localRotation = Quaternion.identity;

            //linkedBox.SetSelected(true);
            linkedToken.detailsWindow = this; // this is a bit hacky. We're saving the window in the card so we don't double-open windows.
                                            // could also track the open windows in tabletop manager instead and check there.

            canvasGroupFader.SetAlpha(0f);
            canvasGroupFader.Show();

            title.text = situationToken.GetTitle();
            description.text = situationToken.GetDescription();

            if (situationToken.isBusy)
            {
                DisplayBusy();
            }

                else
            {
                DisplayReady();
            }
        }

        public SituationToken GetVerbBox() {
            return linkedToken;
        }

        public void ArrangeSlots()
        {
            
            float slotSpacing = 10;
            float slotWidth = ((RectTransform) primarySlot.transform).rect.width;
            float slotHeight = ((RectTransform)primarySlot.transform).rect.height;
            float startingHorizSpace = ((RectTransform) primarySlot.transform.parent).rect.width;
            float startingX = startingHorizSpace/2 - slotWidth;
            float startingY = -120;
            primarySlot.transform.localPosition = new Vector3(startingX, startingY);
            

            if (primarySlot.childSlots.Count > 0)
            {
              
                for (int i = 0; i < primarySlot.childSlots.Count; i++)
                {                   
                    //space needed is space needed for each child slot, + spacing
                    var s = primarySlot.childSlots[i];
                    AlignSlot(s, i, startingX,startingY, slotWidth,slotHeight,slotSpacing);
                }
            }
        }
        
        public void AlignSlot(RecipeSlot thisSlot, int index,float parentX, float parentY,float slotWidth,float slotHeight,float slotSpacing)
        {
            float thisY = parentY - (slotHeight + slotSpacing);
            float spaceNeeded = SlotSpaceNeeded(thisSlot, slotWidth, slotSpacing);
            float thisX = parentX + index*spaceNeeded;
            thisSlot.transform.localPosition = new Vector3(thisX, thisY);
            for (int i = 0; i < thisSlot.childSlots.Count; i++)
            {
                //space needed is space needed for each child slot, + spacing
                var nextSlot = thisSlot.childSlots[i];
                float nextX = thisX + ((slotWidth+slotSpacing)*index);
                AlignSlot(nextSlot, i, nextX,thisY, slotWidth, slotHeight,slotSpacing);
            }

        }

        public float SlotSpaceNeeded(RecipeSlot forSlot,float slotWidth,float slotSpacing)
        {
            float childSpaceNeeded = 0;
            foreach (RecipeSlot c in forSlot.childSlots)
                childSpaceNeeded += SlotSpaceNeeded(c, slotWidth, slotSpacing);

            return Mathf.Max(childSpaceNeeded, slotWidth + slotSpacing);
        }


        RecipeSlot BuildSlot(string slotName = "Recipe Slot", ChildSlotSpecification childSlotSpecification = null) {
            var slot =PrefabFactory.CreateLocally<RecipeSlot>(slotsHolder);
            slot.onCardDropped += HandleOnSlotDroppedOn;
            slot.name = slotName;
            if(childSlotSpecification!=null)
            { 
            slot.GoverningSlotSpecification = childSlotSpecification;
                slot.name += " - " + childSlotSpecification.Label;
            }
            return slot;
        }

        public void Hide()
        {
            canvasGroupFader.Hide();
        }


        void HandleOnSlotDroppedOn(RecipeSlot slot) {

            Debug.Log("Recipe Slot dropped on");

            ElementStack stack=DraggableToken.itemBeingDragged as ElementStack;
            if(stack!=null)
            {
                SlotMatchForAspects match = slot.GetSlotMatchForStack(stack);
                if (match.MatchType == SlotMatchForAspectsType.Okay)
                    StackInSlot(slot, stack);
                else
                    stack.ReturnToTabletop(new Notification {Title = "I can't put that there - ",Description = match.GetProblemDescription() });

            }
        }

        private void StackInSlot(RecipeSlot slot, ElementStack stack)
        {
            DraggableToken.resetToStartPos = false;
            // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
            PositionStackInSlot(slot, stack);

            DisplayRecipeForCurrentAspects();
            stack.Subscribe(this);

            if (stack.HasChildSlots())
                AddSlotsForStack(stack, slot);

            ArrangeSlots();
        }

        private static void PositionStackInSlot(RecipeSlot slot, ElementStack stack)
        {
            stack.transform.SetParent(slot.transform); 
            stack.transform.localPosition = Vector3.zero;
            stack.transform.localRotation = Quaternion.identity;
        }

        private static void ReturnStackToTableTop(ElementStack stack)
        {
           // stack.transform.SetParent(transform); // Make sure to parent back to the tabletop
            stack.transform.localPosition = Vector3.zero;
            stack.transform.localRotation = Quaternion.identity;
        }

        private void AddSlotsForStack(ElementStack stack,RecipeSlot slot)
        {
            foreach (var childSlotSpecification in stack.GetChildSlotSpecifications())
                //add slot to child slots of slot
               slot.childSlots.Add(BuildSlot("childslot of " + stack.ElementId,childSlotSpecification));
        }


        private IDictionary<string, int> GetAspectsFromSlottedCards()
        {
            IDictionary <string, int> currentAspects = GetStacksGatewayForSlots().GetTotalAspects();
            return currentAspects;
        }

        private ElementStacksGateway GetStacksGatewayForSlots()
        {
            return new ElementStacksGateway(new TabletopElementStacksWrapper(slotsHolder));
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
            var recipe = Registry.Compendium.GetFirstRecipeForAspectsWithVerb(aspects, linkedToken.VerbId);
            if(recipe!=null)
            { 
            linkedToken.StoreElementStacks(GetStacksGatewayForSlots().GetStacks());
            linkedToken.BeginSituation(recipe);
            DisplayBusy();
            }

        }

        public void TokenEffectCommandSent(DraggableToken draggableToken, IEffectCommand effectCommand)
        {
            //nothing yet: this may be redundant
        }

        public void TokenPickedUp(DraggableToken draggableToken)
        {
            DisplayRecipeForCurrentAspects();
            draggableToken.Unsubscribe(this);

            RemoveAnyChildSlotsWithEmptyParent();  
            ArrangeSlots();
        }

        private void RemoveAnyChildSlotsWithEmptyParent()
        {
            List<RecipeSlot> currentSlots = new List<RecipeSlot>(GetComponentsInChildren<RecipeSlot>());
            foreach (RecipeSlot s in currentSlots)
            {
                if (s!=null & s.GetElementStackInSlot()==null & s.childSlots.Count > 0)
                {
                    List<RecipeSlot> currentChildSlots = new List<RecipeSlot>(s.childSlots);
                    s.childSlots.Clear();
                    foreach (RecipeSlot cs in currentChildSlots.Where(eachSlot => eachSlot != null))
                        ClearAndDestroySlot(cs);
                }
            }
        }

        private void ClearAndDestroySlot(RecipeSlot slot)
        {
            if (slot == null)
                return;
            //if there are any child slots on this slot, recurse
            if(slot.childSlots.Count>0)
            {
                List<RecipeSlot> childSlots = new List<RecipeSlot>(slot.childSlots);
                foreach (var cs in childSlots)
                    ClearAndDestroySlot(cs);
                slot.childSlots.Clear();
            }
            ElementStack stackContained = slot.GetElementStackInSlot();
            if(stackContained!=null)
            { 
                stackContained.ReturnToTabletop(null);
            }
            DestroyObject(slot.gameObject);
        }

        private void DisplayRecipeForCurrentAspects()
        {
            var currentAspects = GetAspectsFromSlottedCards();
            Recipe r = Registry.Compendium.GetFirstRecipeForAspectsWithVerb(currentAspects, linkedToken.VerbId);
            DisplayRecipe(r);
        }

        public void TokenInteracted(DraggableToken draggableToken)
        {
//currently nothing 
        }

        public void TokenReturnedToTabletop(DraggableToken draggableToken, INotification reason)
        {
           //currently nothing; tokens are automatically returned home
        }
    }
}
