using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries
{
    public class TokenPathItinerary: TokenItinerary

    {
        private Token _travellingToken;


        public override string GetDescription()
        {
            return "🚶";
        }

        public override void Depart(Token tokenToSend, Context context)
        {
            _travellingToken = tokenToSend;

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
    }
}
