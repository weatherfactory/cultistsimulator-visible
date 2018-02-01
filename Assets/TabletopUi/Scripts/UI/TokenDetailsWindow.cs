#pragma warning disable 0649
using System.Collections.Generic;
using Assets.Core;
using Assets.TabletopUi.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace Assets.CS.TabletopUI {
    public class TokenDetailsWindow : BaseDetailsWindow {
        
        // coming in with header "Image" from BaseDetailsWindow
        [SerializeField] GameObject decayView;
        [SerializeField] TextMeshProUGUI decayCountText;

        [Header("Slot Infos")]
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
            // Note: If we want to show the same element, and the token has no timing? Do nothing instead?

            this.element = element;
            this.token = token; // To be able to update the card's remaining time
            this.slotSpec = null;
            Show();
        }

        public void ShowSlotDetails(SlotSpecification slotSpec) {
            this.element = null;
            this.token = null;
            this.slotSpec = slotSpec;
            Show();
        }

        protected override void UpdateContent() {

            if (element != null)
                SetElementCard(element, token);
            else if (slotSpec != null)
                SetSlot(slotSpec);
        }

        void SetElementCard(Element element, ElementStackToken token = null) {
            Sprite sprite;

            if (element.IsAspect)
                sprite = ResourcesManager.GetSpriteForAspect(element.Id);
            else
                sprite = ResourcesManager.GetSpriteForElement(element.Id);

            ShowImage(sprite);
            // NOTE: token is still always NULL
            ShowImageDecayTimer(token != null, token != null ? token.GetCardDecayTime() : null);

            ShowText(elementHeader + element.Label, element.Description);
            SetTextMargin(true, false);
            ShowSlotIcons(false, false); // Make sure the slot icons are gone

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

            aspectsDisplayFlat.DisplayAspects(null);
            aspectsDisplayForbidden.DisplayAspects(slotSpec.Forbidden);
            aspectsDisplayRequired.DisplayAspects(slotSpec.Required);
        }

        void ShowImageDecayTimer(bool show, string timeString = null) {
            decayView.gameObject.SetActive(show);
            decayCountText.text = timeString;
        }

        void SetTextMargin(bool hasImage, bool hasSlots) {
            // We show image, get us a left margin
            title.margin = new Vector4(hasImage ? 80f : 0f, 0f, 0f, 0f);
            // We show slot info? We have less room for the description. Set margin!
            description.margin = new Vector4(hasImage ? 80f : 0f, 0f, 0f, hasSlots ? 30f : 0f);
        }

        void ShowSlotIcons(bool isGreedy, bool consumes) {
            greedyInfo.gameObject.SetActive(isGreedy);
            consumesInfo.gameObject.SetActive(consumes);
        }

    }
}
