using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Enums;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]
    public class EnRouteSphereTangibles: EnRouteSphere
    {
        [SerializeField]
        private EnRouteSphere intangibleAlternative;
        public override Sphere GetAlternativeSphereFor(Token token)
        {
            if (token.OccupiesSpaceAs() != OccupiesSpaceAs.Intangible)
                return this;

            return intangibleAlternative;
        }
    }
}
