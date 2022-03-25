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
       private float _duration;
       private FucinePath _destinationSpherePath;

       public float Duration
       {
           get => _duration;
           protected set => _duration = value;
       }

       public virtual float Elapsed { get; protected set; }

        public abstract string GetDescription();
       public abstract void Depart(Token tokenToSend, Context context);
       public abstract void Depart(Token tokenToSend, Context context, Action<Token, Context> onArrivalCallback);
        public abstract void Arrive(Token tokenToSend, Context context);

        public FucinePath DestinationSpherePath
        {
            get => _destinationSpherePath;
            set => _destinationSpherePath = value;
        }

        public Vector3 Anchored3DStartPosition { get; set; }
       public Vector3 Anchored3DEndPosition { get; set; }
       public abstract IGhost GetGhost();

       public abstract bool IsActive();


   }
}
