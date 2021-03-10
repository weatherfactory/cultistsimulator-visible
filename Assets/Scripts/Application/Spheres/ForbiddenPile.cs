using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Elements;
using SecretHistories.Constants;
using SecretHistories.Services;


namespace SecretHistories.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]
    public class ForbiddenPile : Sphere
    {
        public override SphereCategory SphereCategory => SphereCategory.Dormant;
        public override bool EnforceUniqueStacksInThisContainer => false;
        public override bool ContentsHidden => true;





    }
}
