using System;
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
        public Vector3 startPos { get; set; }
        public Vector3 endPos { get; set; }
        public float startScale { get; set; }
        public float endScale { get; set; }
        //startscale   = 1f, float endScale = 1f)

        public void Depart(Token tokenToSend)
        {
            tokenToSend.Unshroud(true);

            var tokenAnimation = tokenToSend.gameObject.AddComponent<TokenTravelAnimation>();
            tokenAnimation.OnTokenArrival += Arrive;


            //this will cause hilarity if it's applied to a world sphere rather than a threshold
            DestinationSphere.AddBlock(new ContainerBlock(BlockDirection.Inward,
                BlockReason.InboundTravellingStack));

            tokenAnimation.Begin(tokenToSend, Duration);
        }

        public void Arrive(Token token)
        {
            try
            {
                if (DestinationSphere.Equals(null) || DestinationSphere.Defunct)
                    TravelFailed(token);
                else
                    // Assign element to new slot
                    DestinationSphere.AcceptToken(token, new Context(Context.ActionSource.TravelArrived));

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

            token.ReturnToTabletop(new Context(Context.ActionSource.TravelFailed));

        }


        public static TokenTravelItinerary StayExactlyWhereYouAre(Token token)
        {
            return CreateItineraryWithDuration(token.Sphere,token.Sphere,0f,token.Location.Position,token.Location.Position,1f,1f);
        }

        public static TokenTravelItinerary CreateItinerary(Sphere enRouteSphere, Sphere destinationSphere,Vector3 startPos, Vector3 endPos, float startScale, float endScale)
        {
            float distance = Vector3.Distance(startPos, endPos);
           
            float duration = Mathf.Max(0.3f, distance * 0.001f);
            return CreateItineraryWithDuration(enRouteSphere, destinationSphere, duration, startPos, endPos, startScale,
                endScale);
        }


        public static TokenTravelItinerary CreateItineraryWithDuration(Sphere enRouteSphere, Sphere destinationSphere, float duration, Vector3 startPos, Vector3 endPos, float startScale, float endScale)
        {
            var i = new TokenTravelItinerary
            {
                EnRouteSphere = enRouteSphere,
                DestinationSphere = destinationSphere,
                Duration = duration,
                startPos = startPos,
                endPos = endPos,
                startScale = startScale,
                endScale = endScale
            };
            return i;
        }
    }
}