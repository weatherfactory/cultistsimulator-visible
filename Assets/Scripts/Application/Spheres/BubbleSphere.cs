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
        public override IChoreographer Choreographer=>new HorizontalLayoutChoreographer(this); //we don't use a horizontal layout group because - as I learnt to my cost - that moves the anchor positions to (0,0) (top left) and spaffs everything up when they leave the bubblesphere later
        
        

        public void Pop(Context context)
        {
            var tokensToLeaveBehind = new List<Token>(Tokens);

            foreach (var t in tokensToLeaveBehind)
            {
                Watchman.Get<HornedAxe>().GetDefaultSphere().AcceptToken(t,context);
            }
        }


        public override void OnTokenInThisSphereInteracted(TokenInteractionEventArgs args)
        {
            
            Pop(args.Context);


            base.OnTokenInThisSphereInteracted(args);
        }



    }
    }
