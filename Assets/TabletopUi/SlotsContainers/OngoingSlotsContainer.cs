using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;

namespace Assets.TabletopUi.SlotsContainers
{
    public class OngoingSlotsContainer: AbstractSlotsContainer
    {

        /// <param name="sc"></param>
        public override void Initialise( SituationController sc)
        {

            _situationController = sc;
        }

        public void UpdateSlots (IList<SlotSpecification> slotsToBuild)
        {
            IList<RecipeSlot> currentSlots = GetAllSlots();
            foreach (var currentSlot in currentSlots)
            {
                ClearAndDestroySlot(currentSlot);
            }

            if (slotsToBuild.Any())
            {
                gameObject.SetActive(true);
                foreach (SlotSpecification css in slotsToBuild)
                    BuildSlot(css.Label, css);
            }
        }


        public override void RespondToStackAdded(RecipeSlot slot, ElementStack stack)
        {
            _situationController.UpdateSituationDisplay();
        }


        public override void TokenPickedUp(DraggableToken draggableToken)
        {
            _situationController.UpdateSituationDisplay();
            draggableToken.SetContainer(null);

        }

        public IList<IRecipeSlot> GetUnfilledGreedySlots()
        {
            IList <IRecipeSlot> slotsToReturn= new List<IRecipeSlot>();
            foreach (var s in GetAllSlots())
            {
                if(s.GoverningSlotSpecification.Greedy && s.GetElementStackInSlot()==null)
                    slotsToReturn.Add(s);
            }

            return slotsToReturn;
        }
    }
}
