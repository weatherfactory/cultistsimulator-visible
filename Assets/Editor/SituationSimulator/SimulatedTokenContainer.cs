using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.Editor
{
    public class SimulatedTokenContainer : ITokenContainer
    {
        public bool AllowDrag
        {
            get { return false; }
        }

        public bool AllowStackMerge
        {
            get { return false; }
        }

        protected ElementStacksManager ElementStacksManager;

        public SimulatedTokenContainer(string name)
        {
            ElementStacksManager = new ElementStacksManager(this, name);
        }

        public ElementStacksManager GetElementStacksManager()
        {
            return ElementStacksManager;
        }

        public IElementStack ProvisionElementStack(string elementId, int quantity, Source stackSource, string locatorId = null)
        {
            IElementStack stack = new SimulatedElementStack();
            stack.Populate(elementId, quantity, stackSource);
            DisplayHere(stack, new Context(Context.ActionSource.Loading));
            return stack;
        }

        public void DisplayHere(IElementStack stack, Context context)
        {
        }

        public void DisplayHere(DraggableToken token, Context context)
        {
        }

        public void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
        {
            incumbentMoved = false;
        }

        public void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
        {
            incumbentMoved = false;
        }

        public virtual void SignalStackAdded(ElementStackToken elementStackToken, Context context)
        {
        }

        public virtual void SignalStackRemoved(ElementStackToken elementStackToken, Context context)
        {
        }

        public virtual string GetSaveLocationInfoForDraggable(DraggableToken draggable)
        {
            return null;
        }

        public IElementStack ReprovisionExistingElementStack(
            ElementStackSpecification stackSpecification, Source stackSource, string locatorId = null)
        {
            var stack = ProvisionElementStack(stackSpecification.ElementId, stackSpecification.ElementQuantity,
                stackSource, locatorId);
            foreach (var m in stackSpecification.Mutations)
                stack.SetMutation(m.Key, m.Value, false);

            stack.IlluminateLibrarian = new IlluminateLibrarian(stackSpecification.Illuminations);

            if (stackSpecification.LifetimeRemaining > 0)
                stack.LifetimeRemaining = stackSpecification.LifetimeRemaining;

            if (stackSpecification.MarkedForConsumption)
                stack.MarkedForConsumption = true;

            return stack;
        }
    }
}
