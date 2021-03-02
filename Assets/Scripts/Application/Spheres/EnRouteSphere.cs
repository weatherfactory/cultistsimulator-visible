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
using SecretHistories.Constants;
using UnityEngine;

namespace SecretHistories.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]
    public class EnRouteSphere : Sphere
    {

        public override SphereCategory SphereCategory => SphereCategory.World;
        public override float TokenHeartbeatIntervalMultiplier => 1;

        public override bool ProcessEvictedToken(Token token, Context context)
        {
            //accept it before moving it on: the place it's come from may just have been destroyed, so we want it out of harm's way
            AcceptToken(token,context);

            var nextStop = Watchman.Get<HornedAxe>().GetDefaultSphere();
            nextStop.ProcessEvictedToken(token, context);
            return true;
            
        }
    }
}
