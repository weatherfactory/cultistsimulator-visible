using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public TokenPathItinerary()
        { }


        public TokenPathItinerary(TokenLocation startLocation, TokenLocation endLocation)
        {
            Anchored3DStartPosition = startLocation.Anchored3DPosition;
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
            var pathwalkerAnimation = SetupAnimation(tokenToSend,Anchored3DEndPosition);
            pathwalkerAnimation.Begin(tokenToSend,context,1f);

            //lock ghost at ultimate destination
            //          var destinationSphere = Watchman.Get<HornedAxe>().GetSphereByPath(DestinationSpherePath);
            //            var ghost = GetGhost();
            //ghost.ShowAt(destinationSphere,Anchored3DEndPosition,_travellingToken.TokenRectTransform);
        }

        private TokenPathwalkerAnimation SetupAnimation(Token tokenToSend,Vector3 endPosition)
        {
            var tokenAnimation = tokenToSend.gameObject.AddComponent<TokenPathwalkerAnimation>();
            tokenAnimation.EndPosition = endPosition;
            tokenAnimation.OnTokenArrival += Arrive;
            return tokenAnimation;
        }


        public override void Arrive(Token tokenToSend, Context context)
        {
         //
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
