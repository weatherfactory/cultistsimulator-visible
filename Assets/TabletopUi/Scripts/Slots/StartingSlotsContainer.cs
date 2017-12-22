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
    public class StartingSlotsContainer : AbstractSlotsContainer, ITokenContainer {

        [SerializeField] SituationSlotManager slotManager;
        public CanvasGroupFader canvasGroupFader;

        protected RecipeSlot primarySlot;
        private ElementStacksManager _stacksManager;

        public bool AllowStackMerge {
            get { return false; }
        }

        public override void Initialise(SituationController sc) {
            base.Initialise(sc);
            primarySlot = BuildSlot("Primary recipe slot", SlotSpecification.CreatePrimarySlotSpecification(), null);
        }

        public void Reset() {
            if (GetAllSlots().Count > 1) {
                RemoveAnyChildSlotsWithEmptyParent();
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

        public override void RespondToStackPickedUp(IElementStack stack) {
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

                    foreach (RecipeSlot cs in currentChildSlots.Where(eachSlot => eachSlot != null))
                        ClearAndDestroySlot(cs);
                }
            }

            controller.StartingSlotsUpdated();
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
            //In some places we've done it Initialise. Here, we're testing if it's null and then assigning on the fly
            //This is because I'm going through and refactoring. Perhaps it should be consistent YOU TELL ME it's likely to get refactored further anyhoo
            if (_stacksManager == null)
            {
                ITokenTransformWrapper tabletopStacksWrapper = new TokenTransformWrapper(transform);
                _stacksManager = new ElementStacksManager(tabletopStacksWrapper,"startingslot");
            }
            return _stacksManager;
        }


        public void ElementStackRemovedFromContainer(ElementStackToken elementStackToken)
        {
            
        }

        public void TokenDropped(DraggableToken draggableToken) {
        }



        public void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
        {
            //I don't *think* this should ever be called. Let's find out.
            //if it's not, ofc, we have one too few interfaces. The ITokenContainer is being used as both 'thing that has a stacksmanager' and 'direct parent that determines behaviour'
            throw new NotImplementedException();
        }

        public void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
        {
            //I don't *think* this should ever be called. Let's find out.
            //if it's not, ofc, we have one too few interfaces. The ITokenContainer is being used as both 'thing that has a stacksmanager' and 'direct parent that determines behaviour'
            throw new NotImplementedException();
        }


        public string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
            return (draggable.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + draggable.RectTransform.localPosition.y).ToString();
        }
    }

}
