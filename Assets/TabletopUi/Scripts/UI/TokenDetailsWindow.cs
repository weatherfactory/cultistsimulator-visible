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
using System.Linq;
using Assets.Core.Entities;
using TabletopUi.Scripts.Interfaces;

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
        [SerializeField] TextMeshProUGUI greedyInfo;
        [SerializeField] TextMeshProUGUI consumesInfo;
        [SerializeField] Image greedyIcon;
        [SerializeField] Image consumesIcon;

        [Header("Deck Infos")]
        [SerializeField] TextMeshProUGUI deckInfos;

        [Header("Aspect Display")]
        [SerializeField] AspectsDisplay aspectsDisplayFlat;
        [SerializeField] AspectsDisplay aspectsDisplayRequired;
        [SerializeField] AspectsDisplay aspectsDisplayForbidden;

        Coroutine infoHighlight;

        // These are saved here to make sure we have a ref when we're kicking off the anim
        Element _element;
        ElementStackToken _token;

        SlotSpecification slotSpec;

        IDeckSpec deckSpec;
        int deckQuantity;

        public void ShowElementDetails(Element element, ElementStackToken token = null) {

            //AK: removed for now. Mutations complicate things, but also, clicking on the card and getting no response feels stuck
            // Check if we'd show the same, if so: do nothing
            //if (this._element == _element && gameObject.activeSelf) {
            //    if (this.token == token)
            //        return;

            //    bool oldDecays = (this.token != null && this.token.Decays);
            //    bool newDecays = (token != null && token.Decays);

            //    // Is there was and will be no decay visible? Do nothing
            //    if (!oldDecays && !newDecays)
            //        return;
            //}

            SetTokenDecayEventListener(false); // remove decay listener if we had one on an old token
            this._element = element;
            this._token = token; // To be able to update the card's remaining time
            this.slotSpec = null;
            this.deckSpec = null;
            this.deckQuantity = 0;
            Show();
        }

        public void ShowSlotDetails(SlotSpecification slotSpec) {
			/*
            // Check if we'd show the same, if so: do nothing
            if (this.slotSpec == slotSpec && gameObject.activeSelf)
                return;
			*/

            SetTokenDecayEventListener(false); // remove decay listener if we had one on an old token
            this._element = null;
            this._token = null;
            this.slotSpec = slotSpec;
            this.deckSpec = null;
            this.deckQuantity = 0;
            Show();
        }

        public void ShowDeckDetails(IDeckSpec deckSpec, int numCards) {
			/*
            // Check if we'd show the same, if so: do nothing
            if (this.deckSpec == deckSpec && this.deckQuantity == numCards && gameObject.activeSelf)
                return;
			*/

            SetTokenDecayEventListener(false); // remove decay listener if we had one on an old token
            this._element = null;
            this._token = null;
            this.slotSpec = null;
            this.deckSpec = deckSpec;
            this.deckQuantity = numCards;
            Show();
        }

        protected override void ClearContent() {
            SetTokenDecayEventListener(false); // remove decay listener if we had one on an old token

            this._element = null;
            this._token = null;
            this.slotSpec = null;
        }

        void SetTokenDecayEventListener(bool add) {
            if (this._token == null || !this._token.Decays)
                return;

            if (add)
                this._token.onDecay += HandleOnTokenDecay;
            else
                this._token.onDecay -= HandleOnTokenDecay;
        }

        void HandleOnTokenDecay(float timeRemaining) {
            if(_token!=null) //seeing some nullreference errors in the Unity analytics; maybe this is being called after the token is no longer in the window?
                ShowImageDecayTimer(true, _token.GetCardDecayTime());
        }

        protected override void UpdateContent() {
            if (_element != null) {
                SetElementCard(_element, _token);
                SetTokenDecayEventListener(true); // Add decay listener if we need one
            }
            else if (slotSpec != null) {
				SetSlot(slotSpec);
				HighlightSlotCompatibleCards(slotSpec);
            }
            else if (deckSpec != null) {
                SetDeck(deckSpec, deckQuantity);
            }
        }

        // SET TOKEN TYPE CONTENT VISUALS

        void SetElementCard(Element element, ElementStackToken token) {
            Sprite sprite;

            if (element.IsAspect)
                sprite = ResourcesManager.GetSpriteForAspect(element.Icon);
            else
                sprite = ResourcesManager.GetSpriteForElement(element.Icon);

            SetImageNarrow(false);
            ShowImage(sprite);

            if (token != null)
            {
                ShowImageDecayTimer(token.Decays, token.GetCardDecayTime());
                aspectsDisplayFlat.DisplayAspects(
                    token.GetAspects(false)); //token, not _element: cater for possible mutations
            }
            else
                ShowImageDecayTimer(false);

            ShowText(element.Label, element.Description);
            SetTextMargin(true, element.Unique || element.Lifetime > 0); // if the general lifetime is > 0 it decays

            ShowCardIcons(element.Unique, element.Lifetime > 0);
            ShowSlotIcons(false, false); // Make sure the other hint icons are gone
            ShowDeckInfos(0); // Make sure the other hint icons are gone
            aspectsDisplayForbidden.DisplayAspects(null);
            aspectsDisplayRequired.DisplayAspects(null);

        }

        void SetSlot(SlotSpecification slotSpec)
		{
            ShowImage(null);
            ShowImageDecayTimer(false);

			string slotHeader		= LanguageTable.Get("UI_SLOT");
			string slotUnnamed		= LanguageTable.Get("UI_ASPECT");
			string defaultSlotDesc	= LanguageTable.Get("UI_EMPTYSPACE");

            ShowText(
                (string.IsNullOrEmpty(slotSpec.Label) ? slotHeader + slotUnnamed : slotHeader + slotSpec.Label),
                (string.IsNullOrEmpty(slotSpec.Description) ? defaultSlotDesc : slotSpec.Description)
                );
            SetTextMargin(false, slotSpec.Greedy || slotSpec.Consumes);

            ShowCardIcons(false, false); // Make sure the other hint icons are gone
            ShowSlotIcons(slotSpec.Greedy, slotSpec.Consumes);
            ShowDeckInfos(0); // Make sure the other hint icons are gone

            aspectsDisplayFlat.DisplayAspects(null);
            aspectsDisplayForbidden.DisplayAspects(slotSpec.Forbidden);
            aspectsDisplayRequired.DisplayAspects(slotSpec.Required);
        }

        void SetDeck(IDeckSpec deckId, int deckQuantity) {
            var sprite = ResourcesManager.GetSpriteForCardBack(deckId.Id);

            SetImageNarrow(true);
            ShowImage(sprite);
            ShowImageDecayTimer(false);

            ShowText(deckId.Label, deckId.Description);
            SetTextMargin(sprite != null, true);

            ShowCardIcons(false, false); // Make sure the other hint icons are gone
            ShowSlotIcons(false, false); // Make sure the other hint icons are gone
            ShowDeckInfos(deckQuantity);

            aspectsDisplayFlat.DisplayAspects(null);
            aspectsDisplayForbidden.DisplayAspects(null);
            aspectsDisplayRequired.DisplayAspects(null);
        }

        // SUB VISUAL SETTERS

        void SetImageNarrow(bool narrow) {
            artwork.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, narrow ? 65f : 100f);
        }

        void ShowImageDecayTimer(bool show, string timeString = null) {
            decayView.gameObject.SetActive(show);
            decayCountText.text = timeString;
			decayCountText.richText = true;
        }

        void SetTextMargin(bool hasImage, bool hasHints)
		{
            // We show image, get us a left margin - magic number have changed since I rejigged it to allow the aspect area to expand - CP
            title.margin = new Vector4(hasImage ? 100f : 20f, 0f, 50f, 0f);
            // We show slot info? We have less room for the description. Set margin!
            description.margin = new Vector4(hasImage ? 100f : 20f, 0f, 20f, 0f);
        }

        void ShowCardIcons(bool isUnique, bool decays) {
            cardInfoHolder.gameObject.SetActive(isUnique || decays);
            uniqueInfo.gameObject.SetActive(isUnique);
        }

        void ShowSlotIcons(bool isGreedy, bool consumes) {
            slotInfoHolder.gameObject.SetActive(isGreedy || consumes);
            greedyInfo.gameObject.SetActive(isGreedy);
            consumesInfo.gameObject.SetActive(consumes);
        }

        void ShowDeckInfos(int quantity) {
            deckInfos.gameObject.SetActive(quantity > 0);
            deckInfos.text = quantity > 0 ? LanguageTable.Get("UI_UPCOMINGDRAWS") + quantity : null;
        }

        public void HighlightSlotIcon(bool isGreedy, bool consumes) {
            if (infoHighlight != null)
                StopCoroutine(infoHighlight);

            // note can only Highlight one of the two

            if (isGreedy) {
                infoHighlight = StartCoroutine(DoHighlightSlotIcon(greedyIcon, greedyInfo));
            }
            else {
                greedyIcon.transform.localScale = Vector3.one;
                greedyIcon.color = UIStyle.slotDefault;
                greedyInfo.color = UIStyle.textColorLight;

                if (consumes) {
                    infoHighlight = StartCoroutine(DoHighlightSlotIcon(consumesIcon, consumesInfo));
                }
                else {
                    consumesIcon.transform.localScale = Vector3.one;
                    consumesIcon.color = UIStyle.slotDefault;
                    consumesInfo.color = UIStyle.textColorLight;
                }
            }
        }

        IEnumerator DoHighlightSlotIcon(Image icon, TextMeshProUGUI text) {
            const float durationAttack = 0.2f;
            const float durationDecay = 0.4f;
            Vector3 targetScale = new Vector3(1.5f, 1.5f, 1.5f);
            float lerp;

            float time = 0f;

            while (time < durationAttack) {
                time += Time.deltaTime;
                lerp = time / durationAttack;
                icon.transform.localScale = Vector3.Lerp(Vector3.one, targetScale, lerp);
                icon.color = Color.Lerp(UIStyle.slotDefault, Color.black, lerp);
                text.color = Color.Lerp(UIStyle.textColorLight, Color.black, lerp);
                yield return null;
            }

            time = 0f;

            while (time < durationDecay) {
                time += Time.deltaTime;
                lerp = time / durationDecay;
                icon.transform.localScale = Vector3.Lerp(targetScale, Vector3.one, lerp);
                icon.color = Color.Lerp(Color.black, UIStyle.slotDefault, lerp);
                text.color = Color.Lerp(Color.black, UIStyle.textColorLight, lerp);
                yield return null;
            }

            icon.transform.localScale = Vector3.one;
            icon.color = UIStyle.slotDefault;
            text.color = UIStyle.textColorLight;
            infoHighlight = null;
        }

		void HighlightSlotCompatibleCards(SlotSpecification slotSpec) {
			if (slotSpec.Greedy) // Greedy slots get no possible cards
				return;

			var tabletop = Registry.Retrieve<ITabletopManager>();
			tabletop.HighlightAllStacksForSlotSpecificationOnTabletop(slotSpec);
		}
    }
}

