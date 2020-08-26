using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class Limbo : AbstractTokenContainer {

        public void Awake()
        {
            var registry=new Registry();
            registry.Register(this);
        }

        public override void Initialise() {
            _elementStacksManager = new ElementStacksManager(this, "Limbo");
        }
        
        public override string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
            return "limbo";
        }

    }
}

