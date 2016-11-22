using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using UnityEngine;

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

        protected void AddSlotsForStack(ElementStack stack, RecipeSlot slot)
        {
            foreach (var childSlotSpecification in stack.GetChildSlotSpecifications())
                //add slot to child slots of slot
                slot.childSlots.Add(BuildSlot("childslot of " + stack.ElementId, childSlotSpecification));
        }

        public override void StackInSlot(RecipeSlot slot, ElementStack stack)
        {
            DraggableToken.resetToStartPos = false;
            // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
            PositionStackInSlot(slot, stack);

            _situationController.DisplayStartingRecipe();
            stack.SetContainer(this);

            if (stack.HasChildSlots())
                AddSlotsForStack(stack, slot);

            ArrangeSlots();
        }

        public override void TokenPickedUp(DraggableToken draggableToken)
        {
            _situationController.UpdateSituationDisplay();
            draggableToken.SetContainer(null);
            TokenRemovedFromSlot();
        }

        protected void RemoveAnyChildSlotsWithEmptyParent()
        {
            List<RecipeSlot> currentSlots = new List<RecipeSlot>(GetComponentsInChildren<RecipeSlot>());
            foreach (RecipeSlot s in currentSlots)
            {
                if (s != null & s.GetElementStackInSlot() == null & s.childSlots.Count > 0)
                {
                    List<RecipeSlot> currentChildSlots = new List<RecipeSlot>(s.childSlots);
                    s.childSlots.Clear();
                    foreach (RecipeSlot cs in currentChildSlots.Where(eachSlot => eachSlot != null))
                        ClearAndDestroySlot(cs);
                }
            }
        }

        public void TokenRemovedFromSlot()
        {
            RemoveAnyChildSlotsWithEmptyParent();
            ArrangeSlots();
        }


        public void ArrangeSlots()
        {

            float slotSpacing = 10;
            float slotWidth = ((RectTransform)primarySlot.transform).rect.width;
            float slotHeight = ((RectTransform)primarySlot.transform).rect.height;
            float startingHorizSpace = ((RectTransform)primarySlot.transform.parent).rect.width;
            float startingX = startingHorizSpace / 2 - slotWidth;
            float startingY = -120;
            primarySlot.transform.localPosition = new Vector3(startingX, startingY);


            if (primarySlot.childSlots.Count > 0)
            {
                for (int i = 0; i < primarySlot.childSlots.Count; i++)
                {
                    //space needed is space needed for each child slot, + spacing
                    var s = primarySlot.childSlots[i];
                    AlignSlot(s, i, startingX, startingY, slotWidth, slotHeight, slotSpacing);
                }
            }

        }

        protected float SlotSpaceNeeded(RecipeSlot forSlot, float slotWidth, float slotSpacing)
        {
            float childSpaceNeeded = 0;
            foreach (RecipeSlot c in forSlot.childSlots)
                childSpaceNeeded += SlotSpaceNeeded(c, slotWidth, slotSpacing);

            return Mathf.Max(childSpaceNeeded, slotWidth + slotSpacing);
        }



        protected void AlignSlot(RecipeSlot thisSlot, int index, float parentX, float parentY, float slotWidth, float slotHeight, float slotSpacing)
        {
            float thisY = parentY - (slotHeight + slotSpacing);
            float spaceNeeded = SlotSpaceNeeded(thisSlot, slotWidth, slotSpacing);
            float thisX = parentX + index * spaceNeeded;
            thisSlot.transform.localPosition = new Vector3(thisX, thisY);
            for (int i = 0; i < thisSlot.childSlots.Count; i++)
            {
                //space needed is space needed for each child slot, + spacing
                var nextSlot = thisSlot.childSlots[i];
                float nextX = thisX + ((slotWidth + slotSpacing) * index);
                AlignSlot(nextSlot, i, nextX, thisY, slotWidth, slotHeight, slotSpacing);
            }

        }


    }



}
