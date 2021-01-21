using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Interfaces;
using UnityEngine;

namespace SecretHistories.Spheres
{
    public class EnRouteSphere : Sphere
    {

        public override SphereCategory SphereCategory => SphereCategory.World;

        public override bool ProcessEvictedToken(Token token, Context context)
        {
            //accept it before moving it on: the place it's come from may just have been destroyed, so we want it out of harm's way
            AcceptToken(token,context);

            var nextStop = Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere();
            nextStop.ProcessEvictedToken(token, context);
            return true;
            
        }
    }
}
