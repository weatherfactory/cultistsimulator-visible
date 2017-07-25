using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.SlotsContainers
{
    public class StartingSlotsContainer : AbstractSlotsContainer
    {

        public void Reset()
        {
            if(primarySlot==null)
                primarySlot = BuildSlot("Primary recipe slot", SlotSpecification.CreatePrimarySlotSpecification(),null);

            RemoveAnyChildSlotsWithEmptyParent();
            ArrangeSlots();
        }

        protected void AddSlotsForStack(IElementStack stack, RecipeSlot parentSlot)
        {
            foreach (var childSlotSpecification in stack.GetChildSlotSpecifications())
                //add slot to child slots of slot
                parentSlot.childSlots.Add(BuildSlot("childslot of " + stack.Id, childSlotSpecification, parentSlot));
        }

        public override void RespondToStackAdded(RecipeSlot slot, IElementStack stack)
        {
          
            if (stack.HasChildSlots())
                AddSlotsForStack(stack, slot);

            ArrangeSlots();

            _situationController.StartingSlotsUpdated();
        }

        public override void RespondToStackPickedUp(IElementStack stack)
        {
            RemoveAnyChildSlotsWithEmptyParent();
            ArrangeSlots();
            _situationController.StartingSlotsUpdated();
        }

        protected void RemoveAnyChildSlotsWithEmptyParent()
        {
            IList<RecipeSlot> currentSlots = GetAllSlots();
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

            _situationController.StartingSlotsUpdated();
        }

        public void ArrangeSlots()
        {
            int numRows = Mathf.CeilToInt(primarySlot.childSlots.Count / 3f);
            float height = 50 + 35 + (numRows * 120) + ((numRows - 1) * 40);

            (transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }


    }



}
