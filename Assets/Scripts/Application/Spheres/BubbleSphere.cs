using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
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
       [IsEmulousEncaustable(typeof(Sphere))]
    public class BubbleSphere: Sphere
    {
        [SerializeField] private Token AssociatedToken;

        public override SphereCategory SphereCategory => SphereCategory.World;
        public override bool AllowDrag => true;


        public void Pop(Context context)
        {
            var tokensToLeaveBehind = new List<Token>(Tokens);

            foreach (var t in tokensToLeaveBehind)
            {
                Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere().AcceptToken(t,context);
            }
        }


        public override void OnTokenInThisSphereInteracted(TokenInteractionEventArgs args)
        {
            
            Pop(args.Context);


            base.OnTokenInThisSphereInteracted(args);
        }



    }
    }
