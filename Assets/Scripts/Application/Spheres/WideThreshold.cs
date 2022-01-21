using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Enums;
using SecretHistories.Spheres;

namespace SecretHistories.Assets.Scripts.Application.Spheres
{
 [IsEmulousEncaustable(typeof(Sphere))]
    public class WideThreshold: Sphere
    {
        public override SphereCategory SphereCategory => SphereCategory.Threshold;
        public override bool AllowDrag
        {
            get
            {
                return true;
            }
        }
    }
}
