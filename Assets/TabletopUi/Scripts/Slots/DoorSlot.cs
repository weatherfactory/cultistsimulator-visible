using System;
using System.Collections.Generic;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    [ExecuteInEditMode]
    public class DoorSlot : AbstractTokenContainer, IDropHandler, IGlowableView, IPointerEnterHandler, IPointerExitHandler {

        public event System.Action<ElementStackToken> onCardDropped;

        public PortalEffect portalType;
        public Transform[] cardPositions;
        public Image activeGlow;
        public GraphicFader slotGlow;
        public Color defaultBackgroundColor;
        public Image doorColor;
        bool lastGlowState;
        bool isActive;

        const float glowDefaultFactor = 0.65f;

        public override bool AllowDrag { get { return false; } }
        public override bool AllowStackMerge { get { return false; } }

        public void Start()
        {
            Registry.Get<LocalNexus>().TokenInteractionEvent.AddListener(ReactToDraggedToken);
        }

        void ReactToDraggedToken(TokenInteractionEventArgs args)
        {

            if (args.TokenInteractionType == TokenInteractionType.BeginDrag)
            {

                var stack = args.Token as ElementStackToken;

                if (stack == null)
                    return;
                ShowGlow(true, false);
            }


            else if (args.TokenInteractionType == TokenInteractionType.EndDrag)
                ShowGlow(false, false);


        }

        public override void Initialise() {
            ShowGlow(false, true);
            slotGlow.Hide(true);
            //will this be called as necessary? we might need an Initialise()
            _elementStacksManager = new ElementStacksManager(this, "door-"+portalType);
        }

        public string GetDeckName(int cardPosition)
        {
            //cardPosition 0 is the portal itself; >0 is its sub-locations
            string deckId = NoonConstants.MANSUS_DECKID_PREFIX + portalType.ToString().ToLowerInvariant();
            if (cardPosition > 0)
                deckId += cardPosition.ToString();

            return deckId;
        }


        // IGlowableView implementation

        public virtual void OnPointerEnter(PointerEventData eventData) {
            ShowHoverGlow(true);
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
            ShowHoverGlow(false);
        }

        public void SetAsActive(bool active) {
            isActive = active;
            activeGlow.gameObject.SetActive(active);
            doorColor.color = defaultBackgroundColor;
            activeGlow.color = defaultBackgroundColor;
            ShowGlow(false, true);
        }

        public void SetGlowColor(UIStyle.TokenGlowColor colorType) {
            SetGlowColor(UIStyle.GetGlowColor(colorType));
        }

        public void SetGlowColor(Color color) {
            slotGlow.SetColor(color);
        }

        public void ShowGlow(bool glowState, bool instant) {
            if (!isActive)
                glowState = false;

            if (glowState) {
                slotGlow.SetColor(defaultBackgroundColor * glowDefaultFactor);
                slotGlow.Show(instant);
            }
            else { 
                slotGlow.Hide(instant);
            }

            lastGlowState = glowState;
        }

        // Separate method from ShowGlow so we can restore the last state when unhovering
        protected virtual void ShowHoverGlow(bool show) {
            if (!isActive)
                return;


            if (show)
                slotGlow.SetColor(UIStyle.hoverWhite);
            else
                slotGlow.SetColor(defaultBackgroundColor * glowDefaultFactor);
            /*
            if (show)
                slotGlow.Show();
            else
                slotGlow.Hide();
                */
        }

        // IOnDrop Implementation

        public void OnDrop(PointerEventData eventData) {
            if (!isActive || !eventData.dragging)
                return;

            ElementStackToken stack = eventData.pointerDrag.GetComponent<ElementStackToken>();

if(stack!=null)
{

            //now we put the token in the slot.
            AcceptStack(stack, new global::Context(Context.ActionSource.PlayerDrag));
            SoundManager.PlaySfx("CardPutInSlot");
}
        }

        public void AcceptStack(ElementStackToken stack, Context context) {
            GetElementStacksManager().AcceptStack(stack, context);
            // ReSharper disable once PossibleNullReferenceException

            if (onCardDropped != null)
                onCardDropped(stack);
        }

        public AbstractToken GetTokenInSlot() {
            return GetComponentInChildren<AbstractToken>();
        }

        public ElementStackToken GetElementStackInSlot() {
            return GetComponentInChildren<ElementStackToken>();
        }

        public override string GetSaveLocationInfoForDraggable(AbstractToken draggable) {
            throw new NotImplementedException();
        }

#if UNITY_EDITOR
        void OnValidate() {
            ShowGlow(false, true);
        }
#endif

    }
}
