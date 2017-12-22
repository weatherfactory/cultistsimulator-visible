using System;
using System.Collections.Generic;
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
        RecipeSlot ParentSlot { get; set; }
        bool Defunct { get; set; }
        bool Retire();

    }
    public class RecipeSlot : MonoBehaviour, IDropHandler, IRecipeSlot, ITokenContainer, IPointerClickHandler, 
        IGlowableView, IPointerEnterHandler, IPointerExitHandler {
        public event System.Action<RecipeSlot, IElementStack> onCardDropped;
        public event System.Action<IElementStack> onCardPickedUp;
        public SlotSpecification GoverningSlotSpecification { get; set; }
        public IList<RecipeSlot> childSlots { get; set; }
        public RecipeSlot ParentSlot { get; set; }
        public bool Defunct { get; set; }

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
            ITokenTransformWrapper tabletopStacksWrapper = new TokenTransformWrapper(transform);
            _stacksManager = new ElementStacksManager(tabletopStacksWrapper, "slot");
        }

        public void SetSpecification(SlotSpecification slotSpecification) {
            if (slotSpecification == null)
                return;

            SlotLabel.text = slotSpecification.Label;
            GoverningSlotSpecification = slotSpecification;
            IsGreedy = slotSpecification.Greedy;
            IsConsuming = slotSpecification.Consumes;
        }

        // IGlowableView implementation

  

        public virtual void OnPointerEnter(PointerEventData eventData) {
            ShowHoverGlow(true);
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
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
            IElementStack stack = DraggableToken.itemBeingDragged as IElementStack;
            if (stack == null) //it's not an element stack; just put it down
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
            var currentOccupant = GetTokenInSlot();
            if (currentOccupant != null)
                throw new NotImplementedException("There's still a card in the slot when this reaches the slot; it wasn't intercepted by being dropped on the current occupant. Rework.");
                //currentOccupant.ReturnToTabletop();

            //now we put the token in the slot.
            DraggableToken.SetReturn(false,"has gone in slot"); // This tells the draggable to not reset its pos "onEndDrag", since we do that here. (Martin)
            AcceptStack(stack);
            SoundManager.PlaySfx("CardPutInSlot");

            }
        }

        public void AcceptStack(IElementStack stack) {
            ITokenTransformWrapper tabletopStacksWrapper = new TokenTransformWrapper(transform);

            _stacksManager.AcceptStack(stack);

            Assert.IsNotNull(onCardDropped, "no delegate set for cards dropped on recipe slots");
            // ReSharper disable once PossibleNullReferenceException
            onCardDropped(this, stack);
        }


        public DraggableToken GetTokenInSlot() {
            return GetComponentInChildren<DraggableToken>();
        }

        public IElementStack GetElementStackInSlot() {
            return GetComponentInChildren<IElementStack>();
        }

        public SlotMatchForAspects GetSlotMatchForStack(IElementStack stack) {
            if (GoverningSlotSpecification == null)
                return SlotMatchForAspects.MatchOK();
            else
                return GoverningSlotSpecification.GetSlotMatchForAspects(stack.GetAspects());
        }

        public void TokenPickedUp(DraggableToken draggableToken) {
            onCardPickedUp(draggableToken as IElementStack);
        }

        public void TokenDropped(DraggableToken draggableToken) {
        }

        public void TryMoveAsideFor(DraggableToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
        {
            var usurperStack = potentialUsurper as ElementStackToken;
            if (usurperStack == null)
                incumbentMoved = false;
            else
            {
                //incomer is a token. Does it fit in the slot?
                if(GetSlotMatchForStack(usurperStack).MatchType==SlotMatchForAspectsType.Okay)
                { 
                    incumbentMoved = true;
                    incumbent.ReturnToTabletop(); //do this first; AcceptStack will trigger an update on the displayed aspects
                    AcceptStack(usurperStack);
                }
                else
                    incumbentMoved = false;
            }

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

        public ITokenTransformWrapper GetTokenTransformWrapper() {
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
    }
}
