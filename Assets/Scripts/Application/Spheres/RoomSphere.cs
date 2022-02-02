using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Spheres.Angels;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Commands;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]

    public class RoomSphere: Sphere
    {
        private bool _traversable;
        public override bool Traversable => _traversable;

 
        public void Start()
        {
            RoomChoreographer rc = gameObject.GetComponent<RoomChoreographer>();
            
            if (rc!=null && rc.Floors.Any())
                _traversable = true;

        }

        public override SphereCategory SphereCategory => SphereCategory.World;

        public override bool AllowDrag => true;



        public override bool TryDisplayGhost(Token forToken)
        {
            return forToken.DisplayGhostAtChoreographerDrivenPosition(this);

        }
        public override bool TryDisplayGhost(Token forToken, Vector3 overridingWorldPosition)
        {
            return forToken.DisplayGhostAtChoreographerDrivenPosition(this, overridingWorldPosition);
        }
    }
}
