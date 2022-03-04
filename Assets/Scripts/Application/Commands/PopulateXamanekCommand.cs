using SecretHistories.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class PopulateXamanekCommand:IEncaustment
    {
        public Dictionary<string, TokenItinerary> CurrentItineraries { get; set; }=new Dictionary<string,TokenItinerary>();

        public HashSet<SphereBlock> CurrentSphereBlocks { get; set; } = new HashSet<SphereBlock>();



        //Candidate for refactoring. Execute is currently only in use for loading.
        public void Execute(Context context)
        {
            foreach (var ci in CurrentItineraries)
            {
                var token = Watchman.Get<HornedAxe>().FindSingleOrDefaultTokenById(ci.Key);
                if(token==null)
                    NoonUtility.LogWarning($"PopulateXamanekCommand couldn't find a token with the id {ci.Key}, so we can't add an itinerary for that token. This probably means an issue saving from a weird, e.g. otherworld, state.");
                else
                //This doesn't take account of the different spheres the token might be passing through, so we will need to revisit in more detail for BOH
                ci.Value.Depart(token, context);
            }

            var x = Watchman.Get<Xamanek>();

            foreach (var sb in CurrentSphereBlocks)
            {
                x.RegisterSphereBlock(sb);
            }
                
        }
    }
}
