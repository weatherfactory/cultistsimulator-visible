using System;
using System.Collections.Generic;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.Interfaces;
using Assets.TabletopUi.Scripts;
using SecretHistories.Infrastructure;
using SecretHistories.Infrastructure.Events;
using SecretHistories.Interfaces;

using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.UI {
    [ExecuteInEditMode]
    public class DoorSlot : Sphere, IDropHandler, IPointerEnterHandler, IPointerExitHandler {

        public event System.Action<Token> onCardDropped;

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

        public override SphereCategory SphereCategory => SphereCategory.Threshold;

        public void Start()
        {
            ShowGlow(false, true);
            slotGlow.Hide(true);
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

            Token stack = eventData.pointerDrag.GetComponent<Token>();

if(stack!=null)
{

                //now we put the token in the slot.
                AcceptToken(stack, new global::Context(Context.ActionSource.PlayerDrag));
            SoundManager.PlaySfx("CardPutInSlot");
}
        }

        public override void AcceptToken(Token token, Context context) {
            base.AcceptToken(token, context);

            if (onCardDropped != null)
                onCardDropped(token);
        }

        public Token GetTokenInSlot() {
            return GetComponentInChildren<Token>();
        }

        public ElementStack GetElementStackInSlot() {
            return GetComponentInChildren<ElementStack>();
        }

        public override SpherePath GetPath()
        {
            if (!string.IsNullOrEmpty(PathIdentifier))
                NoonUtility.Log($"We're trying to specify a spherepath ({PathIdentifier}) in doorslot for {portalType}");

            return new SpherePath(portalType.ToString());
        }

#if UNITY_EDITOR
        void OnValidate() {
            ShowGlow(false, true);
        }
#endif

    }
}
