using SecretHistories.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Entities;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class PopulateXamanekCommand:IEncaustment
    {
        public Dictionary<string, TokenItinerary> CurrentItineraries { get; set; }

        //Candidate for refactoring. Execute is currently only in use for loading.
        public void Execute(Context context)
        {
            foreach (var ci in CurrentItineraries)
            {
                var token = Watchman.Get<HornedAxe>().FindSingleOrDefaultTokenById(ci.Key);
                //This doesn't take account of the different spheres the token might be passing through, so we will need to revisit in more detail for BOH
                ci.Value.Depart(token, context);
            }
                
        }
    }
}
