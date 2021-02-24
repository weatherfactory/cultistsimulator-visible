using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Spheres
{
    public class LegacyPrimaryThresholdSphereSpecIdentifierStrategy: AbstractSphereSpecIdentifierStrategy
    {
        private const string PRIMARY_SLOT = "primary"; //this is probably now legacy, but may still be necessary for older saved games.

        public override string GetIdentifier()
        {
            return PRIMARY_SLOT;

        }
    }
}
