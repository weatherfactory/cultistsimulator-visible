using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens
{
    public class TokenInertItinerary: TokenItinerary
    {
        private Token _travellingToken;
        private string _tokenName;
        public override string TokenName => _tokenName;

        public override void Depart(Token tokenToSend, Context context)
        {
            _tokenName = tokenToSend.name;
            _travellingToken = tokenToSend;
        }

        public override void Arrive(Token tokenToSend, Context context)
        {
            _travellingToken = tokenToSend;

        }

        public override Rect GetReservedDestinationRect()
        {
            return new Rect(0f, 0f, 0f, 0f);
        }
    }
}
