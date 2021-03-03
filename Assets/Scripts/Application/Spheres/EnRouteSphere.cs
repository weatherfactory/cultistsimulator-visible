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
using SecretHistories.Constants.Events;
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


        public override void OnTokenInThisSphereInteracted(TokenInteractionEventArgs args)
        {
            base.OnTokenInThisSphereInteracted(args);

            if (args.PointerEventData == null || args.Token==null)
                return;
            
            var hovered = args.PointerEventData.hovered;
            foreach (var h in hovered)
            {
                var potentialDropCatcher = h.GetComponent<SphereDropCatcher>();
                if (potentialDropCatcher != null)
                {
                    potentialDropCatcher.DisplayGhost(args.Token);
                    //if (potentialDropCatcher.DisplayGhost(args.Token))
                    //    break;
                }
            }


        }
    }
}
