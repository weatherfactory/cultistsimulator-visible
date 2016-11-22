using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;

namespace Assets.TabletopUi.SlotsContainers
{
    public class OngoingSlotsContainer: AbstractSlotsContainer
    {
        public override void Initialise( SituationController sc)
        {

            _situationController = sc;
            var slotsToBuild = _situationController.GetSlotsForSituation();
            if (slotsToBuild.Any())
            {
                gameObject.SetActive(true);
                foreach (SlotSpecification css in slotsToBuild)
                    BuildSlot(css.Label, css);
            }

        }


        public override void StackInSlot(RecipeSlot slot, ElementStack stack)
        {
            DraggableToken.resetToStartPos = false;
            // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
            PositionStackInSlot(slot, stack);

            _situationController.PredictRecipe();
            stack.SetContainer(this);

        }


        public override void TokenPickedUp(DraggableToken draggableToken)
        {
            _situationController.PredictRecipe();
            draggableToken.SetContainer(null);

        }
    }
}
