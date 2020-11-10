using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class Limbo : Sphere {

        public override ContainerCategory ContainerCategory => ContainerCategory.Dormant;
        public override bool PersistBetweenScenes => true;
        public override bool EnforceUniqueStacksInThisContainer => true;
        public override bool ContentsHidden => true;

        public override string GetPath() {
            return "limbo";
        }

    }
}

