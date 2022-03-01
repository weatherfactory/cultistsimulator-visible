#pragma warning disable 0649
using System.Collections.Generic;
using SecretHistories.Core;
using SecretHistories.UI.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;
using Assets.Scripts.Application.Spheres;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Enums.Elements;
using SecretHistories.Spheres;

namespace SecretHistories.UI {
    public class TokenDetailsWindow : AbstractDetailsWindow {

        // coming in with header "Image" from BaseDetailsWindow
        [SerializeField] GameObject decayView;
        [SerializeField] TextMeshProUGUI decayCountText;

        [Header("Card Infos")]
        [SerializeField] GameObject cardInfoHolder;
        [SerializeField] GameObject uniqueInfo;

        [Header("Deck Infos")]
        [SerializeField] TextMeshProUGUI deckInfos;

        [Header("Aspect Display")]
        [SerializeField] AspectsDisplay aspectsDisplayFlat;

        
        // These are saved here to make sure we have a ref when we're kicking off the anim
        Element _element;
        ElementStack _stack;


        public void ShowTokenDetails(Element element, ElementStack stack) {

            //AK: removed for now. Mutations complicate things, but also, clicking on the card and getting no response feels stuck
            // Check if we'd show the same, if so: do nothing
            //if (this._element == _element && gameObject.activeSelf) {
            //    if (this.token == token
            //        return;

            //    bool oldDecays = (this.token != null && this.token.Decays);
            //    bool newDecays = (token != null && token.Decays);

            //    // Is there was and will be no decay visible? Do nothing
            //    if (!oldDecays && !newDecays)
            //        return;
            //}

            if(_stack!=null)
                _stack.OnLifetimeSpent -= HandleOnTokenDecay;// remove decay listener if we had one on an old token
            this._element = element;
            this._stack = stack; // To be able to update the card's remaining time
            
            Show();
        }


        protected override void ClearContent() {
            _stack.OnLifetimeSpent -= HandleOnTokenDecay; // remove decay listener if we had one on an old token

            this._element = null;
            this._stack = null;
        }



        void HandleOnTokenDecay(float timeRemaining) {
            if(_stack!=null) //seeing some nullreference errors in the Unity analytics; maybe this is being called after the token is no longer in the window?
                ShowImageDecayTimer(true, Watchman.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage(_stack.GetTimeshadow().LifetimeRemaining));
        }

        protected override void UpdateContentAfterNavigation(NavigationArgs args)
        {
            UpdateContent();
        }

        protected override void UpdateContent() {
            if (_element != null) {
                SetElementCard(_element, _stack);
                _stack.OnLifetimeSpent += HandleOnTokenDecay;// Add decay listener if we need one
            }
        }

        // SET TOKEN TYPE CONTENT VISUALS

        void SetElementCard(Element element, ElementStack stack) {
            Sprite sprite;

            if (element.IsAspect)
                sprite = ResourcesManager.GetSpriteForAspect(element.Icon);
            else
                sprite = ResourcesManager.GetSpriteForElement(element.Icon);

            SetImageNarrow(false);
            ShowImage(sprite);

            if (stack != null)
            {
                ShowImageDecayTimer(stack.GetTimeshadow().Transient,
                    Watchman.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage(stack.GetTimeshadow().LifetimeRemaining));
                aspectsDisplayFlat.DisplayAspects(
                    stack.GetAspects(false)); //token, not _element: cater for possible mutations
            }
            else
                ShowImageDecayTimer(false);

            ShowText(element.Label, element.Description);
            SetTextMargin(true, element.Unique || element.Lifetime > 0); // if the general lifetime is > 0 it decays

            ShowCardIcons(element.Unique, element.Lifetime > 0);
           // ShowDeckInfos(0); // Make sure the other hint icons are gone

        }


        void SetDeck(DeckSpec deckId, int deckQuantity) {
            var sprite = ResourcesManager.GetSpriteForCardBack(deckId.Id);

            SetImageNarrow(true);
            ShowImage(sprite);
            ShowImageDecayTimer(false);

            ShowText(deckId.Label, deckId.Description);
            SetTextMargin(sprite != null, true);

            ShowCardIcons(false, false); // Make sure the other hint icons are gone
            ShowDeckInfos(deckQuantity);

            aspectsDisplayFlat.DisplayAspects(null);
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



        void ShowDeckInfos(int quantity) {
            deckInfos.enabled = quantity > 0;
            deckInfos.text = quantity > 0 ? Watchman.Get<ILocStringProvider>().Get("UI_UPCOMINGDRAWS") + quantity : null;
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
            
        }



        private List<Token> FindAllElementTokenssForSlotSpecificationOnTabletop(SphereSpec slotSpec)
        {
            var stackList = new List<Token>();
            var worldSpheres = Watchman.Get<HornedAxe>().GetSpheresOfCategory(SphereCategory.World);
            foreach (var worldSphere in worldSpheres)
            {
                var stackTokens = worldSphere.GetElementTokens();
                foreach (var stackToken in stackTokens)
                    if (slotSpec.CheckPayloadAllowedHere(stackToken.Payload).MatchType == SlotMatchForAspectsType.Okay)
                        stackList.Add(stackToken);
            }

            return stackList;
        }

        private void ShowFXonToken(string name, Transform parent)
        {
            var prefab = Resources.Load(name);

            if (prefab == null)
                return;

            var obj = Instantiate(prefab) as GameObject;

            if (obj == null)
                return;

            obj.transform.SetParent(parent);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.gameObject.SetActive(true);
        }
    }
}

