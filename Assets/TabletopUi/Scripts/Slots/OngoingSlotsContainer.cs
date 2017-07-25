using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.SlotsContainers
{
    public class OngoingSlotsContainer : AbstractSlotsContainer
    {

        public void DestroyAllSlots()
        {
            IList<RecipeSlot> currentSlots = GetAllSlots();
            foreach (var currentSlot in currentSlots)
            {
                ClearAndDestroySlot(currentSlot);
            }

            _situationController.OngoingSlotsUpdated();
        }

        public void SetUpSlots(IList<SlotSpecification> slotsToBuild)
        {
            DestroyAllSlots();
            if (slotsToBuild.Any())
            {
                gameObject.SetActive(true);
                foreach (SlotSpecification css in slotsToBuild)
                    BuildSlot(css.Id, css, null);
            }
        }

        public override void RespondToStackAdded(RecipeSlot slot, IElementStack stack)
        {
            _situationController.OngoingSlotsUpdated();
        }

        public override void RespondToStackPickedUp(IElementStack stack)
        {
            _situationController.OngoingSlotsUpdated();
        }


        public void OnCardPickedUp(DraggableToken draggableToken)
        {
            _situationController.OngoingSlotsUpdated();

        }

        public IRecipeSlot GetUnfilledGreedySlot()
        {
            //we do need the Equals(null); if a slot has been destroyed, it'll still show up here
            //consider moving that filter to GetAllSlots, tho
            var slots = GetAllSlots();
                var candidateSlot = slots.SingleOrDefault();

                if (candidateSlot != null && candidateSlot.GoverningSlotSpecification.Greedy && candidateSlot.GetElementStackInSlot() == null)
                    return candidateSlot;
                else
                    return null;

        }
    }
}
