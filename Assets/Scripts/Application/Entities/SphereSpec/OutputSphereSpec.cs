using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Spheres;

namespace SSecretHistories.Entities
{
    public class OutputSphereSpec : SphereSpec
    {
        public override Type SphereType => typeof(OutputSphere);

        public OutputSphereSpec() : base(new SimpleSphereSpecIdentifierStrategy("output"))
        {

        }
    }
}
