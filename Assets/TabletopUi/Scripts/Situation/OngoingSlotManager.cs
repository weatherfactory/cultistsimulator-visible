#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.TabletopUi;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.TabletopUi.Scripts.Services;

namespace Assets.CS.TabletopUI {
    public class OngoingSlotManager : AbstractSlotsContainer {

        protected RecipeSlot ongoingSlot;
        [SerializeField] Transform slotHolder; 
        [SerializeField] Image countdownBar;
		[SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] LayoutGroup storedCardsLayout;
        public CanvasGroupFader canvasGroupFader;

        public override void Initialise(SituationController sc) {
            base.Initialise(sc);
            ongoingSlot = BuildSlot("ongoing", null, null);
            ongoingSlot.transform.position = slotHolder.transform.position;
        }

        public virtual void Reset() {
            SetupSlot(null);
        }

        public void SetupSlot(Recipe recipe) {
            var slotSpec = (recipe != null && recipe.SlotSpecifications != null && recipe.SlotSpecifications.Count > 0) ? recipe.SlotSpecifications[0] : null;
            ongoingSlot.gameObject.SetActive(slotSpec != null);
            ongoingSlot.Initialise(slotSpec);
        }

        public override void RespondToStackAdded(RecipeSlot slot, IElementStack stack) {
            controller.OngoingSlotsUpdated();
        }

        public override void RespondToStackRemoved(IElementStack stack) {
            controller.OngoingSlotsUpdated();
        }

        public IRecipeSlot GetUnfilledGreedySlot() {
            if (ongoingSlot == null || ongoingSlot.GoverningSlotSpecification == null)
                return null;
            else if (ongoingSlot.GoverningSlotSpecification.Greedy && ongoingSlot.GetElementStackInSlot() == null)
                return ongoingSlot;
            else 
                return null;
        }

        public void UpdateTime(float duration, float timeRemaining, Recipe recipe) {
            Color barColor = UIStyle.GetColorForCountdownBar(recipe);

            countdownBar.color = barColor;
            countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (timeRemaining / duration));
            countdownText.color = barColor;
            countdownText.text = timeRemaining.ToString("0.0") + "s";
        }

        public void ShowStoredAspects(IEnumerable<IElementStack> stacks) {
            int i = 0;

            var aspectFrames = storedCardsLayout.GetComponentsInChildren<AspectFrame>();
            AspectFrame frame;
            Element element;

            foreach (var item in stacks) {
                element = Registry.Retrieve<ICompendium>().GetElementById(item.Id);

                for (int q = 0; q < item.Quantity; q++) {
                    if (i < aspectFrames.Length)
                        frame = aspectFrames[i];
                    else
                        frame = PrefabFactory.CreateLocally<AspectFrame>(storedCardsLayout.transform);

                    frame.PopulateDisplay(element, 1);
                    frame.gameObject.SetActive(true);
                    i++;
                }
            }

            while (i < aspectFrames.Length) {
                aspectFrames[i].gameObject.SetActive(false);

                i++;
            }
        }


    }
}
