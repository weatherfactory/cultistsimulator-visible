using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Spheres.Angels;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres.Angels;
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
    
        public override SphereCategory SphereCategory => SphereCategory.World;
        public override bool AllowDrag => true;
        public override IChoreographer Choreographer=>new ReservedPositionChoreographer(this); 
        
        

        public void Pop(Context context)
        {
            var tokensToLeaveBehind = new List<Token>(Tokens);

            foreach (var t in tokensToLeaveBehind)
            {
                Watchman.Get<HornedAxe>().GetDefaultSphere().AcceptToken(t,context);
            }
        }


        public override void NotifyTokenInThisSphereInteracted(TokenInteractionEventArgs args)
        {
            
            Pop(args.Context);


            base.NotifyTokenInThisSphereInteracted(args);
        }



    }
    }
