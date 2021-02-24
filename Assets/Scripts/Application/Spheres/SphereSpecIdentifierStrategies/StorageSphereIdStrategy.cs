using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Spheres
{
    public class StorageSphereIdStrategy: AbstractSphereSpecIdentifierStrategy
    {
        public override string GetIdentifier()
        {
            return nameof(SituationStorageSphere);
        }


    }
}
