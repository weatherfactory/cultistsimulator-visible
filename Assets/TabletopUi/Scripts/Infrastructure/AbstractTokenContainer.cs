using System;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure {
    public abstract class AbstractTokenContainer : MonoBehaviour, ITokenContainer {

        protected ElementStacksManager _elementStacksManager;

        public virtual bool AllowDrag { get; private set; }
        public virtual bool AllowStackMerge { get; private set; }

        // This is where the ElementStacksManager is created!
        public abstract void Initialise();

        public ElementStacksManager GetElementStacksManager() {
            return _elementStacksManager;
        }

        public IElementStack ReprovisionExistingElementStack(ElementStackSpecification stackSpecification, Source stackSource, string locatorid = null)
        {
            var stack = ProvisionElementStack(stackSpecification.ElementId, stackSpecification.ElementQuantity,
                stackSource, locatorid);
            foreach(var m in stackSpecification.Mutations)
                stack.SetMutation(m.Key,m.Value);

            stack.IlluminateLibrarian=new IlluminateLibrarian(stackSpecification.Illuminations);

            if (stackSpecification.LifetimeRemaining>0)
                stack.LifetimeRemaining = stackSpecification.LifetimeRemaining;

            if (stackSpecification.MarkedForConsumption)
                stack.MarkedForConsumption = true;

            return stack;
        }
        public IElementStack ProvisionElementStack(string elementId, int quantity, Source stackSource, string locatorid = null) {
            IElementStack stack = PrefabFactory.CreateToken<ElementStackToken>(transform, locatorid);
            if (stack == null)
            {
                string stackInfo = "Can't create elementId" + elementId + " from " + stackSource.SourceType;
                if (locatorid != null)
                    stackInfo += " with " + locatorid;

                throw new ApplicationException(stackInfo);
            }


            stack.Populate(elementId, quantity, stackSource);
            DisplayHere(stack, new Context(Context.ActionSource.Loading));
            return stack;
        }

        public virtual void SignalStackAdded(ElementStackToken elementStackToken, Context context) {
            // By default: do nothing right now
        }

        public virtual void SignalStackRemoved(ElementStackToken elementStackToken, Context context) {
            // By default: do nothing right now
        }

        public virtual void DisplayHere(IElementStack stack, Context context) {
            DisplayHere(stack as DraggableToken, context);
        }

        public virtual void DisplayHere(DraggableToken token, Context context) {
            token.transform.SetParent(transform);
            token.transform.localPosition = Vector3.zero;
            token.transform.localRotation = Quaternion.identity;
            token.transform.localScale = Vector3.one;
            token.SetTokenContainer(this, context);
        }

        public virtual void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved) {
            // By default: do no move-aside
            incumbentMoved = false;
        }

        public virtual void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved) {
            // By default: do no move-aside
            incumbentMoved = false;
        }

        abstract public string GetSaveLocationInfoForDraggable(DraggableToken draggable);

        public virtual void OnDestroy() {
            if (_elementStacksManager != null)
                _elementStacksManager.Deregister();
        }
    }
}

