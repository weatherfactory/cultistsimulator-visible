using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class Limbo : MonoBehaviour,IContainsTokensView {
        private ElementStacksManager _elementStacksManager;

        public void Initialise()
        {
            _elementStacksManager=new ElementStacksManager(new TokenTransformWrapper(transform),"Limbo");
        }

        public void SignalElementStackRemovedFromContainer(ElementStackToken elementStackToken)
        {

            //do nothing right now
        }


        public void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
        {
            //do nothing, ever
            incumbentMoved = false;
        }

        public void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
        {
            //do nothing, ever
            incumbentMoved = false;
        }

        public bool AllowDrag { get; private set; }
        public bool AllowStackMerge { get; private set; }
        public ElementStacksManager GetElementStacksManager()
        {
            return _elementStacksManager;
        }

        public string GetSaveLocationInfoForDraggable(DraggableToken draggable)
        {
            return "limbo";
        }

        public void OnDestroy()
        {
            if(_elementStacksManager!=null)
                _elementStacksManager.Deregister();
        }
    }
}

