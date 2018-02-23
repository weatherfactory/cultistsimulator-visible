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
    public class DoorSlot : AbstractTokenContainer, IDropHandler, IGlowableView, IPointerEnterHandler, IPointerExitHandler {

        public event System.Action<IElementStack> onCardDropped;

        public PortalEffect portalType;
        public Transform[] cardPositions;
        public Graphic border;
        public GraphicFader slotGlow;
        bool lastGlowState;

        public override bool AllowDrag { get { return false; } }
        public override bool AllowStackMerge { get { return false; } }

        public override void Initialise() {
            ShowGlow(false, false);
            //will this be called as necessary? we might need an Initialise()
            _elementStacksManager = new ElementStacksManager(this, "door-"+portalType);
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
                SetGlowColor(UIStyle.TokenGlowColor.OnHover);
            else
                SetGlowColor(UIStyle.TokenGlowColor.Default);
        }

        // IOnDrop Implementation

        public void OnDrop(PointerEventData eventData) {
            IElementStack stack = DraggableToken.itemBeingDragged as IElementStack;

            if (stack == null && DraggableToken.itemBeingDragged != null) { //it's not an element stack; just put it down
                DraggableToken.itemBeingDragged.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
                return;
            }

            //now we put the token in the slot.
            DraggableToken.SetReturn(false, "has gone in slot"); // This tells the draggable to not reset its pos "onEndDrag", since we do that here. (Martin)
            AcceptStack(stack, new global::Context(Context.ActionSource.PlayerDrag));
            SoundManager.PlaySfx("CardPutInSlot");
        }

        public void AcceptStack(IElementStack stack, Context context) {
            GetElementStacksManager().AcceptStack(stack, context);
            // ReSharper disable once PossibleNullReferenceException

            if (onCardDropped != null)
                onCardDropped(stack);
        }

        public DraggableToken GetTokenInSlot() {
            return GetComponentInChildren<DraggableToken>();
        }

        public IElementStack GetElementStackInSlot() {
            return GetComponentInChildren<IElementStack>();
        }

        public override string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
            throw new NotImplementedException();
        }

    }
}
