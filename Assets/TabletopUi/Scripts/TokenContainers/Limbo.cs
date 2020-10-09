using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class Limbo : AbstractTokenContainer {

        
        public override void Start() {
            var registry = new Registry();
            registry.Register(this);
            PersistBetweenScenes = true;

            base.Start();
        }
        
        public override string GetSaveLocationInfoForDraggable(AbstractToken draggable) {
            return "limbo";
        }

    }
}

