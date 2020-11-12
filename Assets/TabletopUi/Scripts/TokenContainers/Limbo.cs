using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class Limbo : Sphere {

        public override SphereCategory SphereCategory => SphereCategory.Dormant;
        public override bool PersistBetweenScenes => true;
        public override bool EnforceUniqueStacksInThisContainer => false;
        public override bool ContentsHidden => true;


    }
}

