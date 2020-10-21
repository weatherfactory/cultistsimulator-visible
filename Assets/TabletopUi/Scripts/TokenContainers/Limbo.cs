using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class Limbo : TokenContainer {

        public override ContainerCategory ContainerCategory => ContainerCategory.Dormant;

        public override void Start() {

            PersistBetweenScenes = true;
            base.Start();
        }
        
        public override string GetSaveLocationForToken(AbstractToken token) {
            return "limbo";
        }

    }
}

