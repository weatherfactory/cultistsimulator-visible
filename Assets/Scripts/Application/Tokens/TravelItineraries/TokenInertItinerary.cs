using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Ghosts;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens
{
    public class TokenInertItinerary: TokenItinerary
    {
        private Token _travellingToken;
        

        public override string GetDescription()
        {
            if (_travellingToken == null)
                return "Empty inert TI";
            return $"Inert TI for {_travellingToken.name}";
        }

        public override void Depart(Token tokenToSend, Context context)
        {
            _travellingToken = tokenToSend;
        }

        public override void Arrive(Token tokenToSend, Context context)
        {
            _travellingToken = tokenToSend;

        }

        public override IGhost GetGhost()
        {
            return _travellingToken.GetCurrentGhost();
        }
    }
}
