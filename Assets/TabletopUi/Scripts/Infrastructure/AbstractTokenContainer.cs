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

        /*
        // Not currently in use
        public virtual void SignalElementStackAddedToContainer(ElementStackToken elementStackToken) {
            // By default: do nothing right now
        }
        */

        public IElementStack ProvisionElementStack(string elementId, int quantity, Source stackSource, string locatorid = null) {
            IElementStack stack = PrefabFactory.CreateToken<ElementStackToken>(transform, locatorid);
            stack.Populate(elementId, quantity, stackSource);
            DisplayHere(stack);
            return stack;
        }

        public virtual void SignalStackAdded(ElementStackToken elementStackToken) {
            // By default: do nothing right now
        }

        public virtual void SignalStackRemoved(ElementStackToken elementStackToken) {
            // By default: do nothing right now
        }

        public virtual void DisplayHere(IElementStack stack) {
            DisplayHere(stack as DraggableToken);
        }

        public virtual void DisplayHere(DraggableToken token) {
            token.transform.SetParent(transform);
            token.transform.localPosition = Vector3.zero;
            token.transform.localRotation = Quaternion.identity;
            token.transform.localScale = Vector3.one;
            token.SetTokenContainer(this);
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

