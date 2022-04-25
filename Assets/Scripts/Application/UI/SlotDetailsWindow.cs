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
using Object = UnityEngine.Object;

namespace SecretHistories.UI {
    public class SlotDetailsWindow : AbstractDetailsWindow {


        [Header("Slot Infos")]
        [SerializeField] TextMeshProUGUI greedyInfo;
        [SerializeField] TextMeshProUGUI consumesInfo;
        [SerializeField] Image greedyIcon;
        [SerializeField] Image consumesIcon;


        [Header("Aspect Display")]
        [SerializeField] AspectSetsDisplay aspectsDisplayRequiredAndForbidden;

        [SerializeField]Object cardPingFx;

        private float cardPingLastTriggered = 0.0f;
        SphereSpec slotSpec;

        DeckSpec deckSpec;
        int deckQuantity;

        public void ShowSlotDetails(SphereSpec slotSpec) {
            /*
            // Check if we'd show the same, if so: do nothing
            if (this.slotSpec == slotSpec && gameObject.activeSelf)
                return;
			*/

            this.slotSpec = slotSpec;
            this.deckSpec = null;
            this.deckQuantity = 0;
            Show();
        }


        protected override void ClearContent() {

 
            this.slotSpec = null;
        }





        protected override void UpdateContentAfterNavigation(NavigationArgs args)
        {
            UpdateContent();
        }

        protected override void UpdateContent() {
         if (slotSpec != null) {
				SetSlot(slotSpec);
				HighlightSlotCompatibleCards(slotSpec);
         }
         else if (deckSpec != null) {
             SetDeck(deckSpec, deckQuantity);
         }
        }



        void SetSlot(SphereSpec slotSpec)
		{
            ShowImage(null);
  

			string slotHeader		= Watchman.Get<ILocStringProvider>().Get("UI_SLOT");
			string slotUnnamed		= Watchman.Get<ILocStringProvider>().Get("UI_ASPECT");
			string defaultSlotDesc	= Watchman.Get<ILocStringProvider>().Get("UI_EMPTYSPACE");

            ShowText(
                (string.IsNullOrEmpty(slotSpec.Label) ? slotHeader + slotUnnamed : slotHeader + slotSpec.Label),
                (string.IsNullOrEmpty(slotSpec.Description) ? defaultSlotDesc : slotSpec.Description)
                );
            SetTextMargin(false, slotSpec.Greedy || slotSpec.Consumes);


            ShowSlotIcons(slotSpec.Greedy, slotSpec.Consumes);

            aspectsDisplayRequiredAndForbidden.AddAspectSet(0, slotSpec.Required);
            aspectsDisplayRequiredAndForbidden.AddAspectSet(1, slotSpec.Forbidden);
        }

        void SetDeck(DeckSpec deckId, int deckQuantity) {
            var sprite = ResourcesManager.GetSpriteForCardBack(deckId.Cover);

            SetImageNarrow(true);
            ShowImage(sprite);


            ShowText(deckId.Label, deckId.Description);
            SetTextMargin(sprite != null, true);

 
            ShowSlotIcons(false, false); // Make sure the other hint icons are gone
            aspectsDisplayRequiredAndForbidden.Clear();
        }

        // SUB VISUAL SETTERS

        void SetImageNarrow(bool narrow) {
            artwork.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, narrow ? 65f : 100f);
        }


        void SetTextMargin(bool hasImage, bool hasHints)
		{
            // We show image, get us a left margin - magic number have changed since I rejigged it to allow the aspect area to expand - CP
            title.margin = new Vector4(hasImage ? 100f : 20f, 0f, 50f, 0f);
            // We show slot info? We have less room for the description. Set margin!
            description.margin = new Vector4(hasImage ? 100f : 20f, 0f, 20f, 0f);
        }



        void ShowSlotIcons(bool isGreedy, bool consumes) {
            greedyInfo.gameObject.SetActive(isGreedy);
            consumesInfo.gameObject.SetActive(consumes);
        }

  



		void HighlightSlotCompatibleCards(SphereSpec forSlotSpec) {
			if (forSlotSpec.Greedy) // Greedy slots get no possible cards
				return;

            HighlightAllStacksForSlotSpecificationOnTabletop(forSlotSpec);
		}

  

        public void HighlightAllStacksForSlotSpecificationOnTabletop(SphereSpec forSlotSpec)
        {
            float time = Time.realtimeSinceStartup;
            if (time > cardPingLastTriggered + 1.0f)    // Don't want to trigger these within a second of the last trigger, otherwise they stack up too much
            {
                cardPingLastTriggered = time;

                var stacks = FindAllElementTokenssForSlotSpecificationOnTabletop(forSlotSpec);

                foreach (var stack in stacks)
                {

                    ShowFXonToken(stack.transform);
                }
            }
        }



        private List<Token> FindAllElementTokenssForSlotSpecificationOnTabletop(SphereSpec forSlotSpec)
        {
            var stackList = new List<Token>();
            var worldSpheres = Watchman.Get<HornedAxe>().GetSpheresOfCategory(SphereCategory.World);
            foreach (var worldSphere in worldSpheres)
            {
                var stackTokens = worldSphere.GetElementTokens();
                foreach (var stackToken in stackTokens)
                    if (forSlotSpec.CheckPayloadAllowedHere(stackToken.Payload).MatchType == SlotMatchForAspectsType.Okay)
                        stackList.Add(stackToken);
            }

            return stackList;
        }

        private void ShowFXonToken(Transform parent)
        {
           

            var obj = Instantiate(cardPingFx) as GameObject;

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

