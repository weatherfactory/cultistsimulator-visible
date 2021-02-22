using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Spheres;
using SecretHistories.Abstract;
using SecretHistories.Spheres;

namespace SecretHistories.Entities
{
   public class StorageSphereSpec: SphereSpec
    {

        public override Type SphereType => typeof(SituationStorageSphere);

        public StorageSphereSpec() : base(new SimpleSphereSpecIdentifierStrategy("storage"))
        {

        }

    }
}
