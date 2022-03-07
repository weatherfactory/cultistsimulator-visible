using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Spheres.Angels;
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
        
        public override float TokenHeartbeatIntervalMultiplier => SpecifiedTokenHeartbeatIntervalMultiplier;
        [SerializeField] private float SpecifiedTokenHeartbeatIntervalMultiplier;
        private Sphere _overridingDefaultDestination; //currently in use for otherworlds; tracking a homingAngel reference may be better


        public void Start()
        {
            var unstickAngel = new UnstickerAngel();
            unstickAngel.SetWatch(this);
            AddAngel(unstickAngel);
        }

        public override bool ProcessEvictedToken(Token token, Context context)
        {
             if (flock.MinisterToEvictedToken(token, context))
                return true;

            //accept it before moving it on: the place it's come from may just have been destroyed, so we want it out of harm's way
            AcceptToken(token,context);
            token.MakeInteractable(); //if we got stuck halfway through a drag event, let's make sure we restore raycasting &c
            
            if (_overridingDefaultDestination != null)
              _overridingDefaultDestination.AcceptToken(token,context);
            else
            {
                var nextStop = token.GetHomeSphere(); //eg the current homing angel's location, or a default sphere
                nextStop.ProcessEvictedToken(token, context);
            }
            return true;
            
        }

        public void SetOverridingNextStop(Sphere overridingNextStop)
        {
            _overridingDefaultDestination = overridingNextStop;
        }

        public override void NotifyTokenInThisSphereInteracted(TokenInteractionEventArgs args)
        {

            base.NotifyTokenInThisSphereInteracted(args);


            //when a token is being dragged through it, the EnRoute sphere asks anything underneath to predict interactions were it dropped.
            //we only want to show one predicted interaction, hence the break statements.

            if (args.PointerEventData == null || args.Token==null)
                return;
            
            var hovered = args.PointerEventData.hovered.Where(h=>!h.Equals(null)); //make sure whatever we're hovering over hasn't been destroyed
            foreach (var h in hovered)
            {
                var potentialToken = h.GetComponent<Token>();
                if (potentialToken != null && potentialToken!=args.Token)
                {
                    if (potentialToken.TryShowPredictedInteractionIfDropped(args.Token))
                        break;
                }


                var potentialDropCatcher = h.GetComponent<SphereDropCatcher>();
                if (potentialDropCatcher != null)
                {
                    if (potentialDropCatcher.TryShowPredictedInteractionIfDropped(args.Token))
                        break;
                }
            }


        }
    }
}
