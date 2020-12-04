using System;
using Assets.Core.Entities;
using Assets.Scripts.States.TokenStates;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using UnityEngine;

namespace Assets.CS.TabletopUI
{
    public class TokenTravelItinerary
    {
        public Sphere EnRouteSphere { get; set; }
        public Sphere DestinationSphere { get; set; }
        public float Duration { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }
        public float StartScale { get; set; }
        public float EndScale { get; set; }
        //startscale   = 1f, float endScale = 1f)

        public void Depart(Token tokenToSend)
        {
            tokenToSend.Unshroud(true);
            tokenToSend.SetState(new TravellingState());

            var tokenAnimation = tokenToSend.gameObject.AddComponent<TokenTravelAnimation>();
            tokenAnimation.OnTokenArrival += Arrive;


            //this will cause hilarity if it's applied to a world sphere rather than a threshold
            DestinationSphere.AddBlock(new ContainerBlock(BlockDirection.Inward,
                BlockReason.InboundTravellingStack));

            tokenAnimation.SetPositions(StartPosition,EndPosition);
            tokenAnimation.SetScaling(StartScale,EndScale,1f); //1f was the originally set default. I'm not clear atm about the difference between Duration and ScaleDuration 
            //is it if scaling ends before travel duration?
            tokenAnimation.Begin(tokenToSend, Duration);
        }

        public void Arrive(Token token)
        {
            try
            {
                if (DestinationSphere.Equals(null) || DestinationSphere.Defunct)
                    TravelFailed(token);
                else
                {
                    token.SetState(new TravelledToSphere());
                    // Assign element to new slot
                    DestinationSphere.AcceptToken(token, new Context(Context.ActionSource.TravelArrived));
                }
                DestinationSphere.RemoveBlock(new ContainerBlock(BlockDirection.Inward,
                    BlockReason.InboundTravellingStack));
            }
            catch(Exception e)
            {
                NoonUtility.LogException(e); 
                TravelFailed(token);
            }
        }

        private void TravelFailed(Token token)
        {
            DestinationSphere.RemoveBlock(new ContainerBlock(BlockDirection.Inward,
                BlockReason.InboundTravellingStack));

            token.SetState(new TravellingState() );
            token.ReturnToTabletop(new Context(Context.ActionSource.TravelFailed));

        }

        public static TokenTravelItinerary StayExactlyWhereYouAre(Token token)
        {
           var i=new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D, token.TokenRectTransform.anchoredPosition3D);
           i.StartScale = token.TokenRectTransform.localScale.magnitude;
           i.EndScale= token.TokenRectTransform.localScale.magnitude;
           i.EnRouteSphere = token.Sphere;
           i.DestinationSphere = token.Sphere;
           return i;
        }


        public TokenTravelItinerary(Vector3 startPosition, Vector3 endPosition)
        {
            //the most basic itinerary: don't change sphere, move in current sphere from point to point,s et default duration based on distance, keep current scale
            StartPosition = startPosition;
            EndPosition = endPosition;
            float distance = Vector3.Distance(StartPosition, EndPosition);
            Duration = Mathf.Max(0.3f, distance * 0.001f);
            EnRouteSphere = Registry.Get<SphereCatalogue>().GetDefaultEnRouteSphere();
            DestinationSphere = Registry.Get<SphereCatalogue>().GetDefaultWorldSphere();
        }

        /// <summary>
        /// if we want to cmove to another sphere, set the sphere to travel through and the sphere to end in
        /// </summary>
        /// <param name="enRouteSphere"></param>
        /// <param name="destinationSphere"></param>
        public TokenTravelItinerary WithSphereRoute(Sphere enRouteSphere, Sphere destinationSphere)
        {
            EnRouteSphere = enRouteSphere;
            DestinationSphere = destinationSphere;
            return this;
        }

        public TokenTravelItinerary WithDuration(float duration)
        {
            Duration = duration;
            return this;
        }

        public TokenTravelItinerary WithScaling(float startScale, float endScale)
        {
            StartScale = startScale;
            EndScale = endScale;
            return this;
        }


        
    }
}