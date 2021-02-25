using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Enums;
using SecretHistories.Spheres;

namespace SecretHistories.Spheres
{
    namespace SecretHistories.Spheres
    {
        [IsEmulousEncaustable(typeof(Sphere))]
        public class MinimalSphere : Sphere
        {
            public override SphereCategory SphereCategory => SphereCategory.Meta;
        }
    }
}
