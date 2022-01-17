using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]
    public class PhysicalSphere: Sphere
    {
        public override SphereCategory SphereCategory => SphereCategory.World;

    }
}
