using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    public interface IRecipeSlot {
        IElementStack GetElementStackInSlot();
        DraggableToken GetTokenInSlot();
        SlotMatchForAspects GetSlotMatchForStack(IElementStack stack);
        SlotSpecification GoverningSlotSpecification { get; set; }
        void AcceptStack(IElementStack s);
        string AnimationTag { get; set; }
        RecipeSlot ParentSlot { get; set; }
        bool Defunct { get; set; }
        bool Retire();

    }
    public class RecipeSlot : MonoBehaviour, IDropHandler, IRecipeSlot, IContainsTokensView, IPointerClickHandler, 
        IGlowableView, IPointerEnterHandler, IPointerExitHandler {
        public event System.Action<RecipeSlot, IElementStack> onCardDropped;
        public event System.Action<IElementStack> OnCardRemoved;
        public SlotSpecification GoverningSlotSpecification { get; set; }
        public IList<RecipeSlot> childSlots { get; set; }
        public RecipeSlot ParentSlot { get; set; }
        public bool Defunct { get; set; }
        /// <summary>
        /// This is not yet implemented, nor does it draw from content. I'd like it, if !="", to 
        /// specify an animation used on recipe activation when the element in question is either consumed or moved to recipe storage
        /// </summary>
        public string AnimationTag { get; set; }


        // -----------------------------------------------------------
        // VISUAL ELEMENTS
        public RecipeSlotViz viz;

        public TextMeshProUGUI SlotLabel;
        public Graphic border;
        public GraphicFader slotGlow;
        public LayoutGroup slotIconHolder;

        public GameObject GreedyIcon;
        public GameObject ConsumingIcon;
        public GameObject LockedIcon;

        bool lastGlowState;
        private ElementStacksManager _stacksManager;

        public bool IsGreedy {
            get { return GreedyIcon.activeInHierarchy; }
            set { GreedyIcon.SetActive(value); }
        }

        public bool IsConsuming {
            get { return ConsumingIcon.activeInHierarchy; }
            set { ConsumingIcon.SetActive(value); }
        }

        public bool IsLocked {
            get { return LockedIcon.activeInHierarchy; }
            set { LockedIcon.SetActive(value); }
        }

        public enum SlotModifier { Locked, Ongoing, Greedy, Consuming };

        // TODO: Needs hover feedback!

        public RecipeSlot() {
            childSlots = new List<RecipeSlot>();

        }

        void Start() {
            ShowGlow(false, false);
            IsLocked = false;           
        }

        public void Initialise(SlotSpecification slotSpecification) {
            ITokenPhysicalLocation tabletopStacksWrapper = new TokenTransformWrapper(transform);
            _stacksManager = new ElementStacksManager(tabletopStacksWrapper, "slot");

            if (slotSpecification == null)
                return;

            SlotLabel.text = slotSpecification.Label;
            GoverningSlotSpecification = slotSpecification;
            IsGreedy = slotSpecification.Greedy;
            IsConsuming = slotSpecification.Consumes;
        }

        bool CanInteractWithDraggedObject(DraggableToken token) {
            if (lastGlowState == false) // we're not hoverable? Don't worry
                return false ;

            var stack = token as IElementStack;

            if (stack == null)
                return false; // we only accept stacks

            //does the token match the slot? Check that first
            SlotMatchForAspects match = GetSlotMatchForStack(stack);

            return match.MatchType == SlotMatchForAspectsType.Okay;
        }

        // IGlowableView implementation

        public virtual void OnPointerEnter(PointerEventData eventData) {
            if (lastGlowState && CanInteractWithDraggedObject(DraggableToken.itemBeingDragged))
                DraggableToken.itemBeingDragged.ShowHoveringGlow(true);

            ShowHoverGlow(true); // this only works if the glow was turned on, which was done previously
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
            if (lastGlowState)
                DraggableToken.itemBeingDragged.ShowHoveringGlow(false);

            ShowHoverGlow(false);
        }

        public void SetGlowColor(UIStyle.TokenGlowColor colorType) {
            SetGlowColor(UIStyle.GetGlowColor(colorType));
        }

        public void SetGlowColor(Color color) {
            slotGlow.SetColor(color);
        }

        public void ShowGlow(bool glowState, bool instant) {
            lastGlowState = glowState;

            if (glowState) 
                slotGlow.Show(instant);
            else
                slotGlow.Hide(instant);
        }

        // Separate method from ShowGlow so we can restore the last state when unhovering
        protected virtual void ShowHoverGlow(bool show) {
            // We're NOT dragging something and our last state was not "this is a legal drop target" glow, then don't show
            if (DraggableToken.itemBeingDragged == null && !lastGlowState)
                return;

            if (show)
                SetGlowColor(UIStyle.TokenGlowColor.Hover);
            else 
                SetGlowColor(UIStyle.TokenGlowColor.HighlightPink);
        }


        public bool HasChildSlots() {
            if (childSlots == null)
                return false;
            return childSlots.Count > 0;
        }

        public void OnDrop(PointerEventData eventData) {
            if (DraggableToken.itemBeingDragged == null)
                return;

            Debug.Log("Dropping into " + name + " obj " + DraggableToken.itemBeingDragged);
            IElementStack stack = DraggableToken.itemBeingDragged as IElementStack;

            //it's not an element stack; just put it down
            if (stack == null) 
                DraggableToken.itemBeingDragged.ReturnToTabletop();

            //does the token match the slot? Check that first
            SlotMatchForAspects match = GetSlotMatchForStack(stack);

            if (match.MatchType != SlotMatchForAspectsType.Okay)
            {
                DraggableToken.SetReturn(true,"Didn't match recipe slot values");
                DraggableToken.itemBeingDragged.ReturnToTabletop(new Notification("I can't put that there - ", match.GetProblemDescription()));
            }
            else
            {
                //it matches. Now we check if there's a token already there, and replace it if so:
                var currentOccupant = GetElementStackInSlot();

                // if we drop in the same slot where we came from, do nothing.
                if (currentOccupant == stack) {
                    DraggableToken.SetReturn(true);
                    return;
                }

                if (currentOccupant != null)
                    throw new NotImplementedException("There's still a card in the slot when this reaches the slot; it wasn't intercepted by being dropped on the current occupant. Rework.");
                    //currentOccupant.ReturnToTabletop();

                //now we put the token in the slot.
                DraggableToken.SetReturn(false, "has gone in slot"); // This tells the draggable to not reset its pos "onEndDrag", since we do that here. (Martin)
                AcceptStack(stack);
                SoundManager.PlaySfx("CardPutInSlot");
            }
        }

        public void AcceptStack(IElementStack stack) {
        
            _stacksManager.AcceptStack(stack);

            Assert.IsNotNull(onCardDropped, "no delegate set for cards dropped on recipe slots");
            // ReSharper disable once PossibleNullReferenceException
            onCardDropped(this, stack);
        }



        public DraggableToken GetTokenInSlot() {
            return GetComponentInChildren<DraggableToken>();
        }

        public IElementStack GetElementStackInSlot()
        {
            return _stacksManager.GetStacks().SingleOrDefault();
        }

        public SlotMatchForAspects GetSlotMatchForStack(IElementStack stack) {
            if (GoverningSlotSpecification == null)
                return SlotMatchForAspects.MatchOK();
            else
                return GoverningSlotSpecification.GetSlotMatchForAspects(stack.GetAspects());
        }

        
        public void SignalElementStackRemovedFromContainer(ElementStackToken elementStackToken)
        {
            OnCardRemoved(elementStackToken);
            //PROBLEM: this is called when we return a card to the desktop by clearing another slot! which is not what we want.
            //it puts us in an infinite loop where removing the card from the slot triggers a check for anything else.
            //we want to limit the OnCardPickedUpBehaviour to *only* the card being picked up
            // - or else not have it occur more than once on the same slot? mark as defunct?
            
        }

  
        public void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
        {

                //incomer is a token. Does it fit in the slot?
                if(GetSlotMatchForStack(potentialUsurper).MatchType==SlotMatchForAspectsType.Okay)
                { 
                    incumbentMoved = true;
                    incumbent.ReturnToTabletop(); //do this first; AcceptStack will trigger an update on the displayed aspects
                    AcceptStack(potentialUsurper);
                }
                else
                    incumbentMoved = false;
            

        }
        public void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
        {
            //do nothing, ever
            incumbentMoved = false;
        }



        public bool AllowDrag {
            get {
                return !GoverningSlotSpecification.Greedy;
            }
        }

        public bool AllowStackMerge { get { return false; } }

        public ElementStacksManager GetElementStacksManager()
        {
            return _stacksManager;
        }

        public ITokenPhysicalLocation GetTokenTransformWrapper() {
            return new TokenTransformWrapper(transform);
        }

        /// <summary>
        /// path to slot expressed in underscore-separated slot specification labels: eg "primary_sacrifice"
        /// </summary>
        public string SaveLocationInfoPath {
            get {
                string saveLocationInfo = GoverningSlotSpecification.Id;
                if (ParentSlot != null)
                    saveLocationInfo = ParentSlot.SaveLocationInfoPath + SaveConstants.SEPARATOR + saveLocationInfo;
                return saveLocationInfo;
            }
        }

        public string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
            return SaveLocationInfoPath; //we don't currently care about the actual draggable
        }

        public void SetConsumption() {
            if (GoverningSlotSpecification.Consumes) {
                var stack = GetElementStackInSlot();

                if (stack != null)
                    stack.MarkedForConsumption = true;
            }
        }

        public bool Retire() {
            DestroyObject(gameObject);

            if (Defunct)
                return false;

            Defunct = true;
            return true;
        }

        public void OnPointerClick(PointerEventData eventData) {
            Registry.Retrieve<INotifier>().ShowSlotDetails(GoverningSlotSpecification);
        }

        public void OnDestroy()
        {
            if (_stacksManager != null)
                _stacksManager.Deregister();
        }
    }
}
