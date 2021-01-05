using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Interfaces;
using UnityEngine;

namespace SecretHistories.Infrastructure
{
    public class Limbo : Sphere {

        public override SphereCategory SphereCategory => SphereCategory.Dormant;
        public override bool PersistBetweenScenes => true;
        public override bool EnforceUniqueStacksInThisContainer => false;
        public override bool ContentsHidden => true;


    }
}

