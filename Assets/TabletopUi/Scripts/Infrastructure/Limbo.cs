using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class Limbo : AbstractTokenContainer {

        
        public override void Initialise() {
            var registry = new Registry();
            registry.Register(this);
            _elementStacksManager = new ElementStacksManager(this, "Limbo");
            PersistBetweenScenes = true;
        }
        
        public override string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
            return "limbo";
        }

    }
}

