using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;

namespace Assets.Editor
{
    public abstract class SimulatedSlotsManager
    {
        protected readonly SituationController SituationController;

        protected readonly List<SimulatedRecipeSlot> Slots = new List<SimulatedRecipeSlot>();

        protected SimulatedSlotsManager(SituationController sc)
        {
            SituationController = sc;
        }

        public bool TryAddStackToSlot(string slotId, IElementStack stack)
        {
            // Check if there are available slots
            if (Slots.Count == 0)
                throw new SituationSimulatorException(
                    "Attempted to slot in '" + stack.EntityId + "', but no slots are available");

            // If the slot was not named, it's the primary slot, which is always the first one
            if (slotId == null)
                return Slots[0].TryAddStack(stack);

            // Otherwise, search for the slot by its name
            SimulatedRecipeSlot slot = Slots.Find(s => s.GoverningSlotSpecification.Id == slotId);
            if (slot == null)
                throw new SituationSimulatorException("Failed to find slot with id '" + slotId + "'");
            return slot.TryAddStack(stack);
        }

        public virtual IEnumerable<SimulatedRecipeSlot> GetAllSlots()
        {
            return Slots;
        }

        public AspectsDictionary GetAspectsFromSlottedCards(bool includeElementAspects)
        {
            AspectsDictionary currentAspects = new AspectsDictionary();

            foreach (IRecipeSlot slot in GetAllSlots())
            {
                var stack = slot.GetElementStackInSlot();

                if (stack != null)
                    currentAspects.CombineAspects(stack.GetAspects(includeElementAspects));
            }

            return currentAspects;
        }

        public IEnumerable<IElementStack> GetStacksInSlots()
        {
            return GetAllSlots().Cast<IRecipeSlot>().Select(slot => slot.GetElementStackInSlot()).Where(stack => stack != null).ToList();
        }

        public virtual void DoReset()
        {
        }

        protected SimulatedRecipeSlot BuildSlot(string slotName, SlotSpecification slotSpecification, SimulatedRecipeSlot parentSlot)
        {
            var slot = new SimulatedRecipeSlot(slotSpecification) {Name = slotName, ParentSlot = parentSlot};
            slot.OnCardDropped += RespondToStackAdded;
            slot.OnCardRemoved += RespondToStackRemoved;
            Slots.Add(slot);
            return slot;
        }

        public IRecipeSlot GetSlotBySaveLocationInfoPath(string saveLocationInfoPath)
        {
            var candidateSlots = GetAllSlots();
            IRecipeSlot slotToReturn = candidateSlots.SingleOrDefault(s => s.SaveLocationInfoPath == saveLocationInfoPath);
            return slotToReturn;
        }

        protected abstract void RespondToStackRemoved(IElementStack stack, Context context);

        protected abstract void RespondToStackAdded(SimulatedRecipeSlot slot, IElementStack stack, Context context);
    }

    public class SimulatedStartingSlotsManager : SimulatedSlotsManager
    {
        public SimulatedStartingSlotsManager(SituationController sc) : base(sc)
        {
            var primarySlotSpecification = sc.GetPrimarySlotSpecificationForVerb();
            if (primarySlotSpecification != null)
                BuildSlot(primarySlotSpecification.Label, primarySlotSpecification, null);
            else
                BuildSlot("Primary recipe slot", SlotSpecification.CreatePrimarySlotSpecification(), null);
        }

        protected override void RespondToStackRemoved(IElementStack stack, Context context)
        {
            SituationController.StartingSlotsUpdated();

            if (context.IsManualAction() || context.actionSource == Context.ActionSource.Retire)
                RemoveAnyChildSlotsWithEmptyParent();

            SituationController.StartingSlotsUpdated();
        }

        protected override void RespondToStackAdded(SimulatedRecipeSlot slot, IElementStack stack, Context context)
        {
            SituationController.StartingSlotsUpdated();

            if (slot.IsPrimarySlot() && stack.HasChildSlotsForVerb(SituationController.GetTokenId()))
                AddSlotsForStack(stack, slot);
        }

        public void RemoveAnyChildSlotsWithEmptyParent()
        {
            List<SimulatedRecipeSlot> currentSlots = new List<SimulatedRecipeSlot>(GetAllSlots());

            foreach (SimulatedRecipeSlot s in currentSlots) {
                if (s == null || s.GetElementStackInSlot() != null || s.ChildSlots.Count <= 0)
                    continue;
                List<SimulatedRecipeSlot> currentChildSlots = new List<SimulatedRecipeSlot>(s.ChildSlots);
                s.ChildSlots.Clear();

                foreach (SimulatedRecipeSlot cs in currentChildSlots)
                    ClearAndDestroySlot(cs);
            }

            SituationController.StartingSlotsUpdated();
        }

        private void AddSlotsForStack(IElementStack stack, SimulatedRecipeSlot parentSlot)
        {
            foreach (var childSlotSpecification in stack.GetChildSlotSpecificationsForVerb(SituationController.GetTokenId()))
            {
                var slot = BuildSlot("childslot of " + stack.EntityId, childSlotSpecification, parentSlot);
                parentSlot.ChildSlots.Add(slot);
            }
        }

        private void ClearAndDestroySlot(SimulatedRecipeSlot slot)
        {
            if (slot == null)
                return;
            if (slot.Defunct)
                return;

            Slots.Remove(slot);

            if (slot.ChildSlots.Count <= 0)
                return;
            List<SimulatedRecipeSlot> childSlots = new List<SimulatedRecipeSlot>(slot.ChildSlots);
            foreach (var cs in childSlots)
                ClearAndDestroySlot(cs);

            slot.ChildSlots.Clear();
        }
    }

    public class SimulatedOngoingSlotsManager : SimulatedSlotsManager
    {
        private bool _isActive;

        private readonly SimulatedRecipeSlot _ongoingSlot;

        public SimulatedOngoingSlotsManager(SituationController sc) : base(sc)
        {
            _ongoingSlot = BuildSlot("ongoing", null, null);
            _isActive = false;
        }

        public override IEnumerable<SimulatedRecipeSlot> GetAllSlots()
        {
            return _isActive ? Slots : new List<SimulatedRecipeSlot>(0);
        }

        public override void DoReset()
        {
            SetupSlot(null);
        }

        public void SetupSlot(Recipe recipe) {
            var slotSpec = (recipe != null && recipe.Slots != null && recipe.Slots.Count > 0) ? recipe.Slots[0] : null;
            _ongoingSlot.Initialise(slotSpec);
            _isActive = slotSpec != null;
        }

        protected override void RespondToStackAdded(SimulatedRecipeSlot slot, IElementStack stack, Context context) {
            SituationController.OngoingSlotsOrStorageUpdated();
        }

        protected override void RespondToStackRemoved(IElementStack stack, Context context) {
            SituationController.OngoingSlotsOrStorageUpdated();
        }
    }
}
