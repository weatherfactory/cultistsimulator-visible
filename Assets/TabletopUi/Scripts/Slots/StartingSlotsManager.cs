#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;

namespace Assets.TabletopUi.SlotsContainers {
    public class StartingSlotsManager : AbstractSlotsManager {

        [SerializeField] SlotGridManager gridManager;
        public CanvasGroupFader canvasGroupFader;

        protected RecipeSlot primarySlot;

        public override void Initialise(SituationController sc) {
            base.Initialise(sc);
            primarySlot = BuildSlot("Primary recipe slot", SlotSpecification.CreatePrimarySlotSpecification(), null);
        }

        public void DoReset() {
            if (GetAllSlots().Count > 1) {
                //RemoveAnyChildSlotsWithEmptyParent(); // created an infinite loop - Martin
                ArrangeSlots();
            }
        }

        public override void RespondToStackAdded(RecipeSlot slot, IElementStack stack) {
            if (stack.HasChildSlots())
                AddSlotsForStack(stack, slot);

            ArrangeSlots();
            controller.StartingSlotsUpdated();
        }

        protected void AddSlotsForStack(IElementStack stack, RecipeSlot parentSlot) {
            RecipeSlot slot;

            foreach (var childSlotSpecification in stack.GetChildSlotSpecifications()) {
                slot = BuildSlot("childslot of " + stack.Id, childSlotSpecification, parentSlot);
                parentSlot.childSlots.Add(slot);
            }
        }

        public override void RespondToStackRemoved(IElementStack stack) {
            RemoveAnyChildSlotsWithEmptyParent();
            ArrangeSlots();
            controller.StartingSlotsUpdated();
        }

        protected void RemoveAnyChildSlotsWithEmptyParent() {
            IList<RecipeSlot> currentSlots = GetAllSlots();

            foreach (RecipeSlot s in currentSlots) {
                if (s != null & s.GetElementStackInSlot() == null & s.childSlots.Count > 0) {
                    List<RecipeSlot> currentChildSlots = new List<RecipeSlot>(s.childSlots);
                    s.childSlots.Clear();

                    foreach (RecipeSlot cs in currentChildSlots)
                        ClearAndDestroySlot(cs);
                }
            }

            controller.StartingSlotsUpdated();
        }

        protected override RecipeSlot BuildSlot(string slotName, SlotSpecification slotSpecification, RecipeSlot parentSlot) {
            var slot = base.BuildSlot(slotName, slotSpecification, parentSlot);
            gridManager.AddSlot(slot);
            return slot;
        }

        protected override void ClearAndDestroySlot(RecipeSlot slot) {
            if (slot == null)
                return;
            if (slot.Defunct)
                return;

            // This is all copy & paste from the parent class except for the last line
            if (slot.childSlots.Count > 0) {
                List<RecipeSlot> childSlots = new List<RecipeSlot>(slot.childSlots);
                foreach (var cs in childSlots)
                    ClearAndDestroySlot(cs);

                slot.childSlots.Clear();
            }

            //Destroy the slot *before* returning the token to the tabletop
            //otherwise, the slot will fire OnCardRemoved again, and we get an infinte loop
            gridManager.RetireSlot(slot);

            DraggableToken tokenContained = slot.GetTokenInSlot();

            if (tokenContained != null) 
                tokenContained.ReturnToTabletop(null);
        }

        void ArrangeSlots() {
            gridManager.ReorderSlots();
        }
    }

}
