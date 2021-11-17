using SecretHistories.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class PopulateXamanekCommand:IEncaustment
    {
        public Dictionary<string, TokenItinerary> CurrentItineraries { get; set; }

        public void Execute(Context context)
        {
            foreach(var ci in CurrentItineraries)
                Debug.Log($"{ci.Key} - {ci.Value.DestinationSpherePath}");
        }
    }
}
