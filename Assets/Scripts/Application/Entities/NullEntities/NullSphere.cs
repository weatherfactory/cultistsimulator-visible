using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Enums;
using SecretHistories.Spheres;

namespace Assets.Scripts.Application.Entities.NullEntities
{
   public class NullSphere: Sphere
   {
       public override SphereCategory SphereCategory => SphereCategory.Null;


   }
}
