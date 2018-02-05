#pragma warning disable 0649
using System.Collections.Generic;
using Assets.Core;
using Assets.TabletopUi.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System;

namespace Assets.CS.TabletopUI {
    public class TokenDetailsWindow : BaseDetailsWindow {
        
        // coming in with header "Image" from BaseDetailsWindow
        [SerializeField] GameObject decayView;
        [SerializeField] TextMeshProUGUI decayCountText;

        [Header("Card Infos")]
        [SerializeField] GameObject cardInfoHolder;
        [SerializeField] GameObject uniqueInfo;

        [Header("Slot Infos")]
        [SerializeField] GameObject slotInfoHolder;
        [SerializeField] GameObject greedyInfo;
        [SerializeField] GameObject consumesInfo;

        [Header("Aspect Display")]
        [SerializeField] AspectsDisplay aspectsDisplayFlat;
        [SerializeField] AspectsDisplay aspectsDisplayRequired;
        [SerializeField] AspectsDisplay aspectsDisplayForbidden;

        const string elementHeader = "Card: ";
        const string slotHeader = "Slot: ";
        const string slotUnnamed = "Unnamed Slot";

        // These are saved here to make sure we have a ref when we're kicking off the anim
        Element element;
        ElementStackToken token;
        SlotSpecification slotSpec;

        public void ShowElementDetails(Element element, ElementStackToken token = null) {
            // Check if we'd show the same, if so: do nothing
            if (this.element == element && gameObject.activeSelf) {
                if (this.token == token)
                    return;

                bool oldDecays = (this.token != null && this.token.Decays);
                bool newDecays = (token != null && token.Decays);

                // Is there was and will be no decay visible? Do nothing
                if (!oldDecays && !newDecays)
                    return;
            }

            SetTokenDecayEventListener(false); // remove decay listener if we had one on an old token
            this.element = element;
            this.token = token; // To be able to update the card's remaining time
            this.slotSpec = null;
            Show();
        }

        public void ShowSlotDetails(SlotSpecification slotSpec) {
            // Check if we'd show the same, if so: do nothing
            if (this.slotSpec == slotSpec && gameObject.activeSelf)
                return;

            SetTokenDecayEventListener(false); // remove decay listener if we had one on an old token
            this.element = null;
            this.token = null;
            this.slotSpec = slotSpec;
            Show();
        }

        protected override void ClearContent() {
            SetTokenDecayEventListener(false); // remove decay listener if we had one on an old token

            this.element = null;
            this.token = null;
            this.slotSpec = null;
        }

        void SetTokenDecayEventListener(bool add) {
            if (this.token == null || !this.token.Decays)
                return;

            if (add)
                this.token.onDecay += HandleOnTokenDecay;
            else
                this.token.onDecay -= HandleOnTokenDecay;
        }

        void HandleOnTokenDecay(float timeRemaining) {
            ShowImageDecayTimer(true, token.GetCardDecayTime());
        }

        protected override void UpdateContent() {
            if (element != null) {
                SetElementCard(element, token);
                SetTokenDecayEventListener(true); // Add decay listener if we need one
            }
            else if (slotSpec != null) {
                SetSlot(slotSpec);
            }
        }

        void SetElementCard(Element element, ElementStackToken token = null) {
            Sprite sprite;

            if (element.IsAspect)
                sprite = ResourcesManager.GetSpriteForAspect(element.Id);
            else
                sprite = ResourcesManager.GetSpriteForElement(element.Id);

            ShowImage(sprite);
            ShowImageDecayTimer(token != null && token.Decays, token != null ? token.GetCardDecayTime() : null);

            ShowText(elementHeader + element.Label, element.Description);
            SetTextMargin(true, element.Unique);
            ShowCardIcons(element.Unique);
            ShowSlotIcons(false, false); // Make sure the other hint icons are gone

            aspectsDisplayFlat.DisplayAspects(element.Aspects);
            aspectsDisplayForbidden.DisplayAspects(null);
            aspectsDisplayRequired.DisplayAspects(null);
        }

        void SetSlot(SlotSpecification slotSpec) {
            ShowImage(null);
            ShowImageDecayTimer(false);

            ShowText(slotHeader + (string.IsNullOrEmpty(slotSpec.Label) ? slotUnnamed : slotSpec.Label), slotSpec.Description);
            SetTextMargin(false, slotSpec.Greedy || slotSpec.Consumes);
            ShowSlotIcons(slotSpec.Greedy, slotSpec.Consumes);
            ShowCardIcons(false); // Make sure the other hint icons are gone

            aspectsDisplayFlat.DisplayAspects(null);
            aspectsDisplayForbidden.DisplayAspects(slotSpec.Forbidden);
            aspectsDisplayRequired.DisplayAspects(slotSpec.Required);
        }

        void ShowImageDecayTimer(bool show, string timeString = null) {
            decayView.gameObject.SetActive(show);
            decayCountText.text = timeString;
        }

        void SetTextMargin(bool hasImage, bool hasHints) {
            // We show image, get us a left margin
            title.margin = new Vector4(hasImage ? 80f : 0f, 0f, 0f, 0f);
            // We show slot info? We have less room for the description. Set margin!
            description.margin = new Vector4(hasImage ? 80f : 0f, 0f, 0f, hasHints ? 40f : 0f);
        }

        void ShowCardIcons(bool isUnique) {
            cardInfoHolder.gameObject.SetActive(isUnique);
            uniqueInfo.gameObject.SetActive(isUnique);
        }

        void ShowSlotIcons(bool isGreedy, bool consumes) {
            slotInfoHolder.gameObject.SetActive(isGreedy || consumes);
            greedyInfo.gameObject.SetActive(isGreedy);
            consumesInfo.gameObject.SetActive(consumes);
        }

    }
}
