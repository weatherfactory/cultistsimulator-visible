using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class Limbo : AbstractTokenContainer {

        
        public override void Start() {

            PersistBetweenScenes = true;
            base.Start();
        }
        
        public override string GetSaveLocationForToken(AbstractToken token) {
            return "limbo";
        }

    }
}

