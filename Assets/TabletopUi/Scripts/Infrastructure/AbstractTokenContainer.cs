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
        public virtual bool AlwaysShowHoverGlow { get; private set; }
        public bool PersistBetweenScenes { get; protected set; }

        // This is where the ElementStacksManager is created!
        public abstract void Initialise();

        public ElementStacksManager GetElementStacksManager() {
            return _elementStacksManager;
        }

        public ElementStackToken ReprovisionExistingElementStack(ElementStackSpecification stackSpecification, Source stackSource, Context context, string locatorid = null)
        {
            var stack = ProvisionElementStack(stackSpecification.ElementId, stackSpecification.ElementQuantity,
                stackSource, context, locatorid);
            foreach(var m in stackSpecification.Mutations)
                stack.SetMutation(m.Key,m.Value,false);

            stack.IlluminateLibrarian=new IlluminateLibrarian(stackSpecification.Illuminations);

            if (stackSpecification.LifetimeRemaining>0)
                stack.LifetimeRemaining = stackSpecification.LifetimeRemaining;

            if (stackSpecification.MarkedForConsumption)
                stack.MarkedForConsumption = true;

			stack.LastTablePos = stackSpecification.LastTablePos;

            return stack;
        }
        public virtual ElementStackToken ProvisionElementStack(string elementId, int quantity, Source stackSource, Context context, string locatorid = null) {


            var stack = Registry.Get<PrefabFactory>().CreateLocally<ElementStackToken>(transform);
            stack.AddObserver(Registry.Get<INotifier>());
                
            if (locatorid != null)
                stack.SaveLocationInfo = locatorid;

            stack.Populate(elementId, quantity, stackSource);

            GetElementStacksManager().AcceptStack(stack, context);

            return stack;
        }

        public virtual void SignalStackAdded(ElementStackToken elementStackToken, Context context) {
            // By default: do nothing right now
        }

        public virtual void SignalStackRemoved(ElementStackToken elementStackToken, Context context) {
            // By default: do nothing right now
        }

        public virtual void DisplayHere(ElementStackToken stack, Context context) {
            DisplayHere(stack as AbstractToken, context);
        }

        public virtual void DisplayHere(IToken token, Context context) {
            token.transform.SetParent(transform);
            token.transform.localPosition = Vector3.zero;
            token.transform.localRotation = Quaternion.identity;
            token.transform.localScale = Vector3.one;
            token.SetTokenContainer(this, context);
        }

        public virtual void TryMoveAsideFor(VerbAnchor potentialUsurper, AbstractToken incumbent, out bool incumbentMoved) {
            // By default: do no move-aside
            incumbentMoved = false;
        }

        public virtual void TryMoveAsideFor(ElementStackToken potentialUsurper, AbstractToken incumbent, out bool incumbentMoved) {
            // By default: do no move-aside
            incumbentMoved = false;
        }

        abstract public string GetSaveLocationInfoForDraggable(AbstractToken @abstract);

        public virtual void OnDestroy() {
            if (_elementStacksManager != null)
                _elementStacksManager.Deregister();
        }
    }
}

