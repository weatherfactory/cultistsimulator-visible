using System.Collections.Generic;
using System.Linq;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine.Assertions;

namespace Assets.Editor
{
    public class SimulatedRecipeSlot : SimulatedTokenContainer, IRecipeSlot
    {
        public List<SimulatedRecipeSlot> ChildSlots { get; private set; }
        public IRecipeSlot ParentSlot { get; set; }
        public SlotSpecification GoverningSlotSpecification { get; set; }
        public string AnimationTag { get; set; }
        public bool Defunct { get; set; }
        public event System.Action<SimulatedRecipeSlot, IElementStack, Context> OnCardDropped;
        public event System.Action<IElementStack, Context> OnCardRemoved;

        public string SaveLocationInfoPath {
            get { return null; }
        }
        public string Name { get; set; }

        public SimulatedRecipeSlot(SlotSpecification slotSpecification)
            : base(null)
        {
            Initialise(slotSpecification);
        }

        public void Initialise(SlotSpecification slotSpecification)
        {
            ElementStacksManager = new ElementStacksManager(
                this, slotSpecification != null ? "slotManager-" + slotSpecification.Id : "slotManager");
            GoverningSlotSpecification = slotSpecification;
            Name = slotSpecification != null ? "slot-" + slotSpecification.Id : "slot";
            ChildSlots = new List<SimulatedRecipeSlot>();
        }

        public IElementStack GetElementStackInSlot()
        {
            return ElementStacksManager.GetStacks().SingleOrDefault();
        }

        public DraggableToken GetTokenInSlot()
        {
            return null;
        }

        public bool TryAddStack(IElementStack stack)
        {
            SlotMatchForAspects match = GetSlotMatchForStack(stack);
            if (match.MatchType != SlotMatchForAspectsType.Okay)
                return false;

            if (stack.Quantity > 1)
            {
                stack = stack.SplitAllButNCardsToNewStack(stack.Quantity - 1, new Context(Context.ActionSource.PlayerDrag));
            }
            AcceptStack(stack, new Context(Context.ActionSource.PlayerDrag));
            return true;
        }

        public SlotMatchForAspects GetSlotMatchForStack(IElementStack stack)
        {
            return GoverningSlotSpecification == null ?
                SlotMatchForAspects.MatchOK() : GoverningSlotSpecification.GetSlotMatchForAspects(stack.GetAspects());
        }

        public void AcceptStack(IElementStack s, Context context)
        {
            ElementStacksManager.AcceptStack(s, context);
            Assert.IsNotNull(OnCardDropped, "no delegate set for cards dropped on recipe slots");
            OnCardDropped(this, s, context);
        }

        public bool Retire()
        {
            return true;
        }

        public bool IsPrimarySlot()
        {
            return ParentSlot == null;
        }

        public override void SignalStackRemoved(ElementStackToken elementStackToken, Context context)
        {
            Assert.IsNotNull(OnCardRemoved, "no delegate set for cards removed from recipe slots");
            OnCardRemoved(elementStackToken, context);
        }

        public override string GetSaveLocationInfoForDraggable(DraggableToken draggable)
        {
            return SaveLocationInfoPath;
        }

        public void SetConsumption()
        {
            if (GoverningSlotSpecification.Consumes)
            {
                var stack = GetElementStackInSlot();

                if (stack != null)
                    stack.MarkedForConsumption = true;
            }
        }
    }
}
