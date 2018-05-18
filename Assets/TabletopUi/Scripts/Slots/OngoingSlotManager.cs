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
    public class OngoingSlotManager : AbstractSlotsManager {

        protected RecipeSlot ongoingSlot;
        [SerializeField] Transform slotHolder; 
        [SerializeField] Image countdownBar;
		[SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] LayoutGroup storedCardsLayout;
        public CanvasGroupFader canvasGroupFader;

        [SerializeField] DeckEffectView[] deckEffectViews; 

        public override void Initialise(SituationController sc) {
            base.Initialise(sc);
            ongoingSlot = BuildSlot("ongoing", null, null);
            SetSlotToPos();
        }

        public override IList<RecipeSlot> GetAllSlots() {
            // Is the active slot enabled?
            if (ongoingSlot.gameObject.activeSelf)
                return base.GetAllSlots();
            else
                return new List<RecipeSlot>(0);
        }

        public void SetSlotToPos() {
            ongoingSlot.transform.position = slotHolder.position;
        }

        public virtual void DoReset() {
            SetupSlot(null);
        }

        public void SetupSlot(Recipe recipe) {
            var slotSpec = (recipe != null && recipe.SlotSpecifications != null && recipe.SlotSpecifications.Count > 0) ? recipe.SlotSpecifications[0] : null;
            ongoingSlot.gameObject.SetActive(slotSpec != null);
            ongoingSlot.Initialise(slotSpec);
        }

        public override void RespondToStackAdded(RecipeSlot slot, IElementStack stack, Context context) {
            situationController.OngoingSlotsUpdated();
        }

        public override void RespondToStackRemoved(IElementStack stack, Context context) {
            situationController.OngoingSlotsUpdated();
        }

        public IRecipeSlot GetUnfilledGreedySlot() {
            if (ongoingSlot == null || ongoingSlot.GoverningSlotSpecification == null)
                return null;
            //code smell here. Why is the slot either null or GoverningSlotSpecificationNull? How does it get into each situation?
            //Possibly it should be set to null again when returned to starting state?
            //there was an issue where once created, a greedy slot would never be retired, and would always grab whenever that verb was used
            //I resolved this by setting GoverningSlotSpecification to null when null is passed to SetupSlot, but this is all a bit fragile. - AK

            else if (ongoingSlot.GoverningSlotSpecification.Greedy && ongoingSlot.GetElementStackInSlot() == null)
                return ongoingSlot;
            else 
                return null;
        }

        public void UpdateTime(float duration, float timeRemaining, EndingFlavour forEndingFlavour) {
            Color barColor = UIStyle.GetColorForCountdownBar(forEndingFlavour, timeRemaining);

            countdownBar.color = barColor;
            countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (timeRemaining / duration));
            countdownText.color = barColor;
            countdownText.text = "<mspace=1.6em>" + timeRemaining.ToString("0.0") + "s";
            countdownText.richText = true;
        }

        public void ShowStoredAspects(IEnumerable<IElementStack> stacks) {
            int i = 0;

            var aspectFrames = storedCardsLayout.GetComponentsInChildren<ElementFrame>();
            ElementFrame frame;
            Element element;

            foreach (var stack in stacks) {
                element = Registry.Retrieve<ICompendium>().GetElementById(stack.EntityId);

                for (int q = 0; q < stack.Quantity; q++) {
                    if (i < aspectFrames.Length)
                        frame = aspectFrames[i];
                    else
                        frame = PrefabFactory.CreateLocally<ElementFrame>(storedCardsLayout.transform);

                    frame.PopulateDisplay(element,1, stack as ElementStackToken);
                    frame.gameObject.SetActive(true);
                    i++;
                }
            }

            while (i < aspectFrames.Length) {
                aspectFrames[i].gameObject.SetActive(false);

                i++;
            }
        }

        public void ShowDeckEffects(Dictionary<string, int> deckEffects) {
            // Note, We shouldn't have more effects than we have views
            UnityEngine.Assertions.Assert.IsTrue(deckEffects.Count <= deckEffectViews.Length);

            int i = 0;
            IDeckSpec deckSpec;

            // Populate those we need
            foreach (var item in deckEffects) {
                deckSpec = Registry.Retrieve<ICompendium>().GetDeckSpecById(item.Key);
                deckEffectViews[i].PopulateDisplay(deckSpec, item.Value);
                deckEffectViews[i].gameObject.SetActive(true);
                i++;
            }


            // All those we didn't need? Hide them.
            while (i < deckEffectViews.Length) {
                deckEffectViews[i].gameObject.SetActive(false);
                i++;
            }
        }

    }
}
