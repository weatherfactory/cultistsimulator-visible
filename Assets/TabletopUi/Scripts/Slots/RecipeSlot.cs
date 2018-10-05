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
        void AcceptStack(IElementStack s, Context context);
        string AnimationTag { get; set; }
        RecipeSlot ParentSlot { get; set; }
        bool Defunct { get; set; }
        bool Retire();
        bool IsPrimarySlot();
    }

    public class RecipeSlot : AbstractTokenContainer, IDropHandler, IRecipeSlot, IPointerClickHandler, IGlowableView, IPointerEnterHandler, IPointerExitHandler {

        public event System.Action<RecipeSlot, IElementStack, Context> onCardDropped;
        public event System.Action<IElementStack, Context> onCardRemoved;

        // DATA ACCESS
        public SlotSpecification GoverningSlotSpecification { get; set; }
        public IList<RecipeSlot> childSlots { get; set; }
        public RecipeSlot ParentSlot { get; set; }
        public bool Defunct { get; set; }
        public string AnimationTag { get; set; }

        // VISUAL ELEMENTS
        public RecipeSlotViz viz;

        public TextMeshProUGUI SlotLabel;
        public Graphic border;
        public GraphicFader slotGlow;
        public LayoutGroup slotIconHolder;

        public GameObject GreedyIcon;
        public GameObject ConsumingIcon;

        bool lastGlowState;

        public bool IsBeingAnimated { get; set; }

        public override bool AllowStackMerge { get { return false; } }

        public override bool AllowDrag {
            get {
                return !GoverningSlotSpecification.Greedy || IsBeingAnimated;
            }
        }

        public bool IsGreedy
        {
            get { return GoverningSlotSpecification != null && GoverningSlotSpecification.Greedy; }
        }

        public bool IsConsuming
        {
            get { return GoverningSlotSpecification.Consumes; }
        }

        public enum SlotModifier { Locked, Ongoing, Greedy, Consuming };

        public RecipeSlot() {
            childSlots = new List<RecipeSlot>();
        }

        void Start() {
            ShowGlow(false, false);
        }

        public override void Initialise() {
            throw new NotImplementedException(); // We have a separate init function here.
        }

        public void Initialise(SlotSpecification slotSpecification) {
            _elementStacksManager = new ElementStacksManager(this, "slot");
            GoverningSlotSpecification = slotSpecification;
            //we need to do this first. Code checks if an ongoing slot is active by checking whether it has a slotspecification
            //slots with null slotspecification are inactive.

            if (slotSpecification == null)
                return;

            SlotLabel.text = slotSpecification.Label;
            
            GreedyIcon.SetActive(slotSpecification.Greedy);
            ConsumingIcon.SetActive(slotSpecification.Consumes);
        }

		bool CanInteractWithDraggedObject(DraggableToken token) {
            if (lastGlowState == false || token == null) // we're not hoverable? Don't worry
                return false;

            var stack = token as IElementStack;

            if (stack == null)
                return false; // we only accept stacks

            //does the token match the slot? Check that first
            SlotMatchForAspects match = GetSlotMatchForStack(stack);

            return match.MatchType == SlotMatchForAspectsType.Okay;
        }

        // IGlowableView implementation

		public virtual void OnPointerEnter(PointerEventData eventData) {
			if (GoverningSlotSpecification.Greedy) // we're greedy? No interaction.
				return;

            if (DraggableToken.itemBeingDragged == null) {
                if (GetTokenInSlot() == null) // Only glow if the slot is empty
                    ShowHoverGlow(true);
            }
            else if (CanInteractWithDraggedObject(DraggableToken.itemBeingDragged)) { 
                if (lastGlowState)
                    DraggableToken.itemBeingDragged.ShowHoveringGlow(true);

                if (GetTokenInSlot() == null) // Only glow if the slot is empty
                    ShowHoverGlow(true);
            }
        }

		public virtual void OnPointerExit(PointerEventData eventData) {
			if (GoverningSlotSpecification.Greedy) // we're greedy? No interaction.
				return;
			
            if (lastGlowState && DraggableToken.itemBeingDragged != null)
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
            //if (DraggableToken.itemBeingDragged == null && !lastGlowState)
            //    return;
            
            if (show) { 
                SetGlowColor(UIStyle.TokenGlowColor.OnHover);
                SoundManager.PlaySfx("TokenHover");
                slotGlow.Show();
            }
            else { 
                SetGlowColor(UIStyle.TokenGlowColor.Default);
                //SoundManager.PlaySfx("TokenHoverOff");

                if (lastGlowState)
                    slotGlow.Show();
                else
                    slotGlow.Hide();
            }
        }

        public bool HasChildSlots() {
            if (childSlots == null)
                return false;
            return childSlots.Count > 0;
        }

		public void OnDrop(PointerEventData eventData) {
			if (GoverningSlotSpecification.Greedy) // we're greedy? No interaction.
				return;
			
            if (IsBeingAnimated || DraggableToken.itemBeingDragged == null || !(DraggableToken.itemBeingDragged is ElementStackToken))
                return;

            NoonUtility.Log("Dropping into " + name + " obj " + DraggableToken.itemBeingDragged,10);
            IElementStack stack = DraggableToken.itemBeingDragged as IElementStack;

            //it's not an element stack; just put it down
            if (stack == null) 
                DraggableToken.itemBeingDragged.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));

            //does the token match the slot? Check that first
            SlotMatchForAspects match = GetSlotMatchForStack(stack);

            if (match.MatchType != SlotMatchForAspectsType.Okay)
            {
                DraggableToken.SetReturn(true, "Didn't match recipe slot values");
                DraggableToken.itemBeingDragged.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));

                var notifier = Registry.Retrieve<INotifier>();

                var compendium = Registry.Retrieve<ICompendium>();

                if (notifier != null)
                    notifier.ShowNotificationWindow( LanguageTable.Get("UI_CANTPUT"), match.GetProblemDescription(compendium));
            }
            else if (stack.Quantity != 1) {
                // We're dropping more than one?
                // So make sure the card we're dropping that is being returned to it's position
                DraggableToken.SetReturn(true);
                // And we split a new one that's 1 (leaving the returning card to be n-1)
                var newStack = stack.SplitAllButNCardsToNewStack(stack.Quantity - 1, new Context(Context.ActionSource.PlayerDrag));
                // And we put that into the slot
                AcceptStack(newStack, new Context(Context.ActionSource.PlayerDrag));
            }
            else  {
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
                AcceptStack(stack, new Context(Context.ActionSource.PlayerDrag));
                SoundManager.PlaySfx("CardPutInSlot");
            }
        }

        public void AcceptStack(IElementStack stack, Context context) {
            _elementStacksManager.AcceptStack(stack, context);

            Assert.IsNotNull(onCardDropped, "no delegate set for cards dropped on recipe slots");
            // ReSharper disable once PossibleNullReferenceException
            onCardDropped(this, stack, context);
        }

        public override void DisplayHere(DraggableToken token, Context context) {
            base.DisplayHere(token, context);
            var stack = token as ElementStackToken;

            if (stack != null) {
                stack.ShowCardShadow(false); // no shadow in slots
                slotIconHolder.transform.SetAsLastSibling();
            }
        }

        public DraggableToken GetTokenInSlot() {
            return GetComponentInChildren<DraggableToken>();
        }

        public IElementStack GetElementStackInSlot()
        {
            return _elementStacksManager.GetStacks().SingleOrDefault();
        }

        public SlotMatchForAspects GetSlotMatchForStack(IElementStack stack) {
            if (GoverningSlotSpecification == null)
                return SlotMatchForAspects.MatchOK();
            else
                return GoverningSlotSpecification.GetSlotMatchForAspects(stack.GetAspects());
        }
        
        public override void SignalStackRemoved(ElementStackToken elementStackToken, Context context) {
            onCardRemoved(elementStackToken, context);
            //PROBLEM: this is called when we return a card to the desktop by clearing another slot! which is not what we want.
            //it puts us in an infinite loop where removing the card from the slot triggers a check for anything else.
            //we want to limit the OnCardPickedUpBehaviour to *only* the card being picked up
            // - or else not have it occur more than once on the same slot? mark as defunct?
        }
  
        public override void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved) {
            if (IsGreedy) { // We do not allow 
                incumbentMoved = false;
                return;
            }

            //incomer is a token. Does it fit in the slot?
            if (GetSlotMatchForStack(potentialUsurper).MatchType==SlotMatchForAspectsType.Okay)
            { 
                incumbentMoved = true;
                incumbent.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag)); //do this first; AcceptStack will trigger an update on the displayed aspects
                AcceptStack(potentialUsurper, new Context(Context.ActionSource.PlayerDrag));
            }
            else
                incumbentMoved = false;
        }



        /// <summary>
        /// path to slot expressed in underscore-separated slot specification labels: eg "work_sacrifice"
        /// </summary>
        public string SaveLocationInfoPath {
            get {
                string saveLocationInfo = GoverningSlotSpecification.Id;
                if (ParentSlot != null)
                    saveLocationInfo = ParentSlot.SaveLocationInfoPath + SaveConstants.SEPARATOR + saveLocationInfo;
                return saveLocationInfo;
            }
        }

        public override string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
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

        public bool IsPrimarySlot()
        {
            return ParentSlot == null;
        }

        public void OnPointerClick(PointerEventData eventData) {
            bool highlightGreedy = GreedyIcon.gameObject.activeInHierarchy && eventData.hovered.Contains(GreedyIcon);
            bool highlightConsumes = ConsumingIcon.gameObject.activeInHierarchy && eventData.hovered.Contains(ConsumingIcon);

            Registry.Retrieve<INotifier>().ShowSlotDetails(GoverningSlotSpecification, highlightGreedy, highlightConsumes);

        }

    }
}
