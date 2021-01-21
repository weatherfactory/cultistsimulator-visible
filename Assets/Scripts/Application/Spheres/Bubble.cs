using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Enums;

namespace SecretHistories.Spheres
{

    /// <summary>
    /// A bubble bursts, dropping all its contents, when interacted with.
    /// </summary>
    public class Bubble: Sphere
    {
        public override SphereCategory SphereCategory => SphereCategory.World;
    }
}
