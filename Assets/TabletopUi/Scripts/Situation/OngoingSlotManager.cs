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

namespace Assets.CS.TabletopUI {
    public class OngoingSlotManager : AbstractSlotsContainer {

        protected RecipeSlot ongoingSlot;
        [SerializeField] Transform slotHolder; 
        [SerializeField] Image countdownBar;
		[SerializeField] TextMeshProUGUI countdownText;
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
            ongoingSlot.SetSpecification(slotSpec);
        }

        public override void RespondToStackAdded(RecipeSlot slot, IElementStack stack) {
            _situationController.OngoingSlotsUpdated();
        }

        public override void RespondToStackPickedUp(IElementStack stack) {
            _situationController.OngoingSlotsUpdated();
        }

        public IRecipeSlot GetUnfilledGreedySlot() {
            if (ongoingSlot == null || ongoingSlot.GoverningSlotSpecification == null)
                return null;
            else if (ongoingSlot.GoverningSlotSpecification.Greedy && ongoingSlot.GetElementStackInSlot() == null)
                return ongoingSlot;
            else 
                return null;
        }

        public void UpdateTime(float duration, float timeRemaining) {
            countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (timeRemaining / duration));
            countdownText.text = timeRemaining.ToString("0.0") + "s";
        }
    }
}
