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
    public class LibraryEnRouteSphereTangibles: EnRouteSphere
    {
        [SerializeField]
        private EnRouteSphere intangibleAlternative;
        public override Sphere GetAlternativeSphereFor(Token token)
        {
            if (token.OccupiesSpaceAs() != OccupiesSpaceAs.Intangible)
                return this;

            return intangibleAlternative;
        }

        //public override bool ProcessEvictedToken(Token token, Context context)
        //{

        //    if (flock.MinisterToEvictedToken(token, context))
        //        return true;

        //    //accept it before moving it on: the place it's come from may just have been destroyed, so we want it out of harm's way
        //    AcceptToken(token, context);
        //    token.MakeInteractable(); //if we got stuck halfway through a drag event, let's make sure we restore raycasting &c

        //    if (_overridingDefaultDestination != null)
        //        _overridingDefaultDestination.AcceptToken(token, context);
        //    else
        //    {
        //        var nextStop = token.GetHomeSphere(); //eg the current homing angel's location, or a default sphere
        //        nextStop.ProcessEvictedToken(token, context);
        //    }
        //    return true;

        //}
    }
}
