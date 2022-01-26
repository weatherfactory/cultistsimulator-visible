using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]

    public class RoomSphere: Sphere
    {

        private Dictionary<string, FucinePath> _exits;


        public override SphereCategory SphereCategory => SphereCategory.World;

        public override bool AllowDrag => true;
        public override bool TryDisplayGhost(Token forToken)
        {
            return forToken.DisplayGhostAtChoreographerDrivenPosition(this);

        }
    }
}
