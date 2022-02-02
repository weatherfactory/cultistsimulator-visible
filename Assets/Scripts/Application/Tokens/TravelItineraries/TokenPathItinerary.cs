using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pathfinding;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;

using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries
{
    public class TokenPathItinerary: TokenItinerary

    {
        private Token _travellingToken;
        private Path AStarPath;
        public TokenPathItinerary()
        { }


        public TokenPathItinerary(TokenLocation startLocation, TokenLocation endLocation)
        {
            Anchored3DStartPosition = startLocation.Anchored3DPosition; //Is this unused??
            Anchored3DEndPosition = endLocation.Anchored3DPosition;
            DestinationSpherePath = endLocation.AtSpherePath;
        }

        public TokenPathItinerary(Vector3 anchored3DStartPosition, Vector3 anchored3DEndPosition)
        {
            Anchored3DStartPosition = anchored3DStartPosition;
            Anchored3DEndPosition = anchored3DEndPosition;

            DestinationSpherePath = Watchman.Get<HornedAxe>().GetDefaultSpherePath();
        }

        public override string GetDescription()
        {
            return "🚶";
        }

        public override void Depart(Token tokenToSend, Context context)
        {

            Depart(tokenToSend,context,null);

        }

        public override void Depart(Token tokenToSend, Context context, Action<Token, Context> onArrivalCallback)
        {
            _travellingToken = tokenToSend;
            
            
            var seeker = tokenToSend.gameObject.GetComponent<Seeker>();
            var tokenAi = tokenToSend.gameObject.gameObject.GetComponent<TokenAILerp>();
            
            tokenAi.destination= Anchored3DEndPosition;
            
           // AStarPath = seeker.StartPath(tokenToSend.transform.position, Anchored3DEndPosition);
    tokenAi.OnTokenArrival += Arrive;

            _travellingToken.transform.SetAsLastSibling();

        }


        public override void Arrive(Token tokenToSend, Context context)
        {
            
            var tokenAILerp = tokenToSend.gameObject.gameObject.GetComponent<TokenAILerp>();
            tokenAILerp.OnTokenArrival -= Arrive;
            
            _travellingToken.OnCompletedTravelItinerary();
            _travellingToken.HideGhost();
        }

        public override IGhost GetGhost()
        {
            return _travellingToken.GetCurrentGhost();

        }

        public override bool IsActive()
        {
            return true;
        }

        public TokenPathItinerary WithDestinationSpherePath(FucinePath destinationSpherePath)
        {
            DestinationSpherePath = destinationSpherePath;
            return this;
        }
    }
}
