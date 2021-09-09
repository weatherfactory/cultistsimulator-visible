using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries
{
   public abstract class AbstractItinerary
   {
       public abstract void Depart(Token tokenToSend, Context context);
       public abstract void Arrive(Token tokenToSend, Context context);


    }
}
