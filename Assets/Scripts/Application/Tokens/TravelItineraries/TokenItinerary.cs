using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries
{
   public abstract class TokenItinerary
   {
       public abstract string GetDescription();
       public abstract void Depart(Token tokenToSend, Context context);
       public abstract void Arrive(Token tokenToSend, Context context);
       public FucinePath DestinationSpherePath { get; set; }
       public Vector3 Anchored3DStartPosition { get; set; }
       public Vector3 Anchored3DEndPosition { get; set; }
       public abstract IGhost GetGhost();

   }
}
