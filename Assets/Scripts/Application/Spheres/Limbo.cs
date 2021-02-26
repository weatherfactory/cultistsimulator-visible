using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]
    public class Limbo : Sphere {

        public override SphereCategory SphereCategory => SphereCategory.Dormant;
        public override bool PersistBetweenScenes => true;
        public override bool EnforceUniqueStacksInThisContainer => false;
        public override bool ContentsHidden => true;

        public override bool IsInRangeOf(Sphere otherSphere)
        {
            return false;
        }

    }
}

