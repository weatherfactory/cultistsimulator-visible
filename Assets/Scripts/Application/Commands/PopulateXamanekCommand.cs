using SecretHistories.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class PopulateXamanekCommand:IEncaustment
    {
        public Dictionary<string, TokenItinerary> CurrentItineraries { get; set; }
    }
}
