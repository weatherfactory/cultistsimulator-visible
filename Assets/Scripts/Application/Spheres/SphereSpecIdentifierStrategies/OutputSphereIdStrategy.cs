using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Spheres;

namespace SecretHistories.SphereSpecIdentifierStrategies
{
    public class OutputSphereIdStrategy: AbstractSphereSpecIdentifierStrategy
    {
        public override string GetIdentifier()
        {
            return nameof(OutputSphere);
        }

    }
}
