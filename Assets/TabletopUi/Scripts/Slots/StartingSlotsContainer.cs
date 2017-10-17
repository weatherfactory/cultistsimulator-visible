﻿using System;
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
    public class StartingSlotsContainer : AbstractSlotsContainer, ITokenContainer {

        protected RecipeSlot primarySlot;
        [SerializeField] SituationSlotManager slotManager;
        public CanvasGroupFader canvasGroupFader;

        public bool AllowStackMerge {
            get { return false; }
        }

        public void Reset() {
            if (primarySlot == null)
                primarySlot = BuildSlot("Primary recipe slot", SlotSpecification.CreatePrimarySlotSpecification(), null);

            if (GetAllSlots().Count > 1) {
                RemoveAnyChildSlotsWithEmptyParent();
                ArrangeSlots();
            }
        }

        public override void RespondToStackAdded(RecipeSlot slot, IElementStack stack) {
            if (stack.HasChildSlots())
                AddSlotsForStack(stack, slot);

            ArrangeSlots();

            _situationController.StartingSlotsUpdated();
        }

        protected void AddSlotsForStack(IElementStack stack, RecipeSlot parentSlot) {
            RecipeSlot slot;

            foreach (var childSlotSpecification in stack.GetChildSlotSpecifications()) {
                slot = BuildSlot("childslot of " + stack.Id, childSlotSpecification, parentSlot);
                parentSlot.childSlots.Add(slot);
            }
        }

        public override void RespondToStackPickedUp(IElementStack stack) {
            RemoveAnyChildSlotsWithEmptyParent();
            ArrangeSlots();
            _situationController.StartingSlotsUpdated();
        }

        protected void RemoveAnyChildSlotsWithEmptyParent() {
            IList<RecipeSlot> currentSlots = GetAllSlots();

            foreach (RecipeSlot s in currentSlots) {
                if (s != null & s.GetElementStackInSlot() == null & s.childSlots.Count > 0) {
                    List<RecipeSlot> currentChildSlots = new List<RecipeSlot>(s.childSlots);
                    s.childSlots.Clear();

                    foreach (RecipeSlot cs in currentChildSlots.Where(eachSlot => eachSlot != null))
                        ClearAndDestroySlot(cs);
                }
            }

            _situationController.StartingSlotsUpdated();
        }

        protected override RecipeSlot BuildSlot(string slotName, SlotSpecification slotSpecification, RecipeSlot parentSlot) {
            var slot = base.BuildSlot(slotName, slotSpecification, parentSlot);
            slotManager.AddSlot(slot);
            return slot;
        }

        protected override void ClearAndDestroySlot(RecipeSlot slot) {
            if (slot == null)
                return;

            // This is all copy & paste from the parent class except for the last line
            if (slot.childSlots.Count > 0) {
                List<RecipeSlot> childSlots = new List<RecipeSlot>(slot.childSlots);
                foreach (var cs in childSlots)
                    ClearAndDestroySlot(cs);

                slot.childSlots.Clear();
            }

            DraggableToken tokenContained = slot.GetTokenInSlot();

            if (tokenContained != null) 
                tokenContained.ReturnToTabletop(null);

            slotManager.RemoveSlot(slot);
        }

        void ArrangeSlots() {
            slotManager.ReorderSlots();
        }

        public ElementStacksManager GetElementStacksManager() {
            ITokenTransformWrapper stacksWrapper = new TokenTransformWrapper(transform);
            return new ElementStacksManager(stacksWrapper);
        }

        public void TokenPickedUp(DraggableToken draggableToken) {
        }

        public void TokenDropped(DraggableToken draggableToken) {
        }

        public string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
            return (draggable.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + draggable.RectTransform.localPosition.y).ToString();
        }
    }

}
