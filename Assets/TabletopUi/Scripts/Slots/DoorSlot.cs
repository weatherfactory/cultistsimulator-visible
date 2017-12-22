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
    public class DoorSlot : MonoBehaviour, IDropHandler, ITokenContainer, IGlowableView, IPointerEnterHandler, IPointerExitHandler {
        public event System.Action< IElementStack> onCardDropped;

        public Graphic border;
        public GraphicFader slotGlow;
        bool lastGlowState;
        private ElementStacksManager _stacksManager;


        void Start() {
            ShowGlow(false, false);
            ITokenTransformWrapper stacksWrapper = new TokenTransformWrapper(transform);
            //will this be called as necessary? we might need an Initialise()
            _stacksManager = new ElementStacksManager(stacksWrapper,"door");
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

        public void OnDrop(PointerEventData eventData) {
            IElementStack stack = DraggableToken.itemBeingDragged as IElementStack;

            if (stack == null) { //it's not an element stack; just put it down
                DraggableToken.itemBeingDragged.ReturnToTabletop();
                return;
            }

            //now we put the token in the slot.
            DraggableToken.SetReturn(false, "has gone in slot"); // This tells the draggable to not reset its pos "onEndDrag", since we do that here. (Martin)
            AcceptStack(stack);
            SoundManager.PlaySfx("CardPutInSlot");
        }

        public void AcceptStack(IElementStack stack) {
            GetElementStacksManager().AcceptStack(stack);
            // ReSharper disable once PossibleNullReferenceException
            onCardDropped(stack);
        }

        public DraggableToken GetTokenInSlot() {
            return GetComponentInChildren<DraggableToken>();
        }

        public IElementStack GetElementStackInSlot() {
            return GetComponentInChildren<IElementStack>();
        }

        public void ElementStackRemovedFromContainer(ElementStackToken elementStackToken)
        {
         
        }

  public bool AllowDrag {
            get {
                return false;
            }
        }

        public bool AllowStackMerge { get { return false; } }

        public ElementStacksManager GetElementStacksManager()
        {
            return _stacksManager;
        }


        public void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
        {
            //do nothing, ever
            incumbentMoved = false;
        }

        public void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
        {
            //do nothing, ever
            incumbentMoved = false;
        }

        public string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
            throw new NotImplementedException();
        }
    }
}
