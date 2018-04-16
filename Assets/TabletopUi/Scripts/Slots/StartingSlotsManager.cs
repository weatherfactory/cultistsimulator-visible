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

        public override void RespondToStackAdded(RecipeSlot slot, IElementStack stack, Context context) {

            // startingSlots updated may resize window
            situationController.StartingSlotsUpdated();

            if (stack.HasChildSlotsForVerb(situationController.GetTokenId()))
                AddSlotsForStack(stack, slot);

            ArrangeSlots();

        }

        protected void AddSlotsForStack(IElementStack stack, RecipeSlot parentSlot) {
            RecipeSlot slot;

            foreach (var childSlotSpecification in stack.GetChildSlotSpecificationsForVerb(situationController.GetTokenId())) {
                slot = BuildSlot("childslot of " + stack.Id, childSlotSpecification, parentSlot);
                parentSlot.childSlots.Add(slot);
            }
        }

        public override void RespondToStackRemoved(IElementStack stack, Context context) {
            // startingSlots updated may resize window
            situationController.StartingSlotsUpdated();

            // Only update the slots if we're doing this manually, otherwise don't
            if (context.IsManualAction())
                RemoveAnyChildSlotsWithEmptyParent(context);

            ArrangeSlots();
            situationController.StartingSlotsUpdated();

        }

        public void RemoveAnyChildSlotsWithEmptyParent(Context context) {
            // We get a copy of the list, since it modifies itself when slots are removed
            List<RecipeSlot> currentSlots = new List<RecipeSlot>(GetAllSlots());

            foreach (RecipeSlot s in currentSlots) {
                if (s != null & s.GetElementStackInSlot() == null & s.childSlots.Count > 0) {
                    List<RecipeSlot> currentChildSlots = new List<RecipeSlot>(s.childSlots);
                    s.childSlots.Clear();

                    foreach (RecipeSlot cs in currentChildSlots)
                        ClearAndDestroySlot(cs, context);
                }
            }

            situationController.StartingSlotsUpdated();
        }

        protected override RecipeSlot BuildSlot(string slotName, SlotSpecification slotSpecification, RecipeSlot parentSlot) {
            var slot = base.BuildSlot(slotName, slotSpecification, parentSlot);
            gridManager.AddSlot(slot);
            return slot;
        }

        protected override void ClearAndDestroySlot(RecipeSlot slot, Context context) {
            if (slot == null)
                return;
            if (slot.Defunct)
                return;

            validSlots.Remove(slot);

            // This is all copy & paste from the parent class except for the last line
            if (slot.childSlots.Count > 0) {
                List<RecipeSlot> childSlots = new List<RecipeSlot>(slot.childSlots);
                foreach (var cs in childSlots)
                    ClearAndDestroySlot(cs, context);

                slot.childSlots.Clear();
            }

            //Destroy the slot *before* returning the token to the tabletop
            //otherwise, the slot will fire OnCardRemoved again, and we get an infinte loop
            gridManager.RetireSlot(slot);

            if (context != null && context.actionSource == Context.ActionSource.SituationStoreStacks)
                return; // Don't return the tokens to tabletop if we

            DraggableToken tokenContained = slot.GetTokenInSlot();

            if (tokenContained != null) 
                tokenContained.ReturnToTabletop(context);
        }

        void ArrangeSlots() {
            gridManager.ReorderSlots();
        }

        public void SetGridNumPerRow() {
            gridManager.SetNumPerRow();
        }
    }

}
