using Assets.TabletopUi.Scripts.Infrastructure;
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

        public static TokenTravelItinerary StayExactlyWhereYouAre(Token token)
        {
            return new TokenTravelItinerary(token.Sphere,token.Sphere,0f,token.Location.Position,token.Location.Position,1f,1f);
        }

        public TokenTravelItinerary(Sphere enRouteSphere, Sphere destinationSphere, float duration, Vector3 startPos, Vector3 endPos, float startScale, float endScale)
        {
            EnRouteSphere = enRouteSphere;
            DestinationSphere = destinationSphere;
            Duration = duration;
            this.startPos = startPos;
            this.endPos = endPos;
            this.startScale = startScale;
            this.endScale = endScale;
        }
    }
}