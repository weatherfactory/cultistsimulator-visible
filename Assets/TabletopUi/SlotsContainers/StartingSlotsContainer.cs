using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;

namespace Assets.TabletopUi.SlotsContainers
{
    public class StartingSlotsContainer : AbstractSlotsContainer
    {
        public override void Initialise(SituationController sc)
        {


            _situationController = sc;
            gameObject.SetActive(true);
            primarySlot = BuildSlot();
            ArrangeSlots();

        }

        public override void StackInSlot(RecipeSlot slot, ElementStack stack)
        {
            DraggableToken.resetToStartPos = false;
            // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
            PositionStackInSlot(slot, stack);

            _situationController.DisplayRecipeForAllSlottedAspects();
            stack.SetContainer(this);

            if (stack.HasChildSlots())
                AddSlotsForStack(stack, slot);

            ArrangeSlots();
        }

        public override void TokenPickedUp(DraggableToken draggableToken)
        {
            _situationController.DisplayRecipeForAllSlottedAspects();
            draggableToken.SetContainer(null);
            TokenRemovedFromSlot();
        }
    }

    

}
