using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Spheres
{

    /// <summary>
    /// A bubble dumps its contents to the world sphere when any of its contents receive an interaction event
    /// </summary>
    public class BubbleSphere: Sphere
    {
        [SerializeField] private Token AssociatedToken;

        public override SphereCategory SphereCategory => SphereCategory.World;
        public override bool AllowDrag => true;


        public void Pop(Context context)
        {
            var tokensToLeaveBehind = new List<Token>(GetAllTokens());

            foreach (var t in tokensToLeaveBehind)
            {
                var startLocation = new TokenLocation(t);
                var endLocation =
                    startLocation.WithSpherePath(Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere().GetPath());

                var i = new TokenTravelItinerary(startLocation, endLocation);

                i.Depart(t, context);
            }
        }


        public override void OnTokenInThisSphereInteracted(TokenInteractionEventArgs args)
        {
            
            Pop(args.Context);


            base.OnTokenInThisSphereInteracted(args);
        }



    }
    }
