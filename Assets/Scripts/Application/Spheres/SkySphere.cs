using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Manifestations;
using SecretHistories.Spheres;

namespace SecretHistories.Assets.Scripts.Application.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]

    public class SkySphere: PhysicalSphere
    {
        public override Type GetShabdaManifestation(Situation situation)
        {
            return typeof(SunManifestation);
        }
    }
}
