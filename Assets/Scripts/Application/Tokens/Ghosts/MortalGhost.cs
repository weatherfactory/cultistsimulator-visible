using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Spheres.Angels;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Entities;
using SecretHistories.Ghosts;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Tokens.Ghosts
{
    public class MortalGhost: AbstractGhost
    {



        ///Get a path itinerary based on the ghost's current position. Return the token to its
        /// original location if it's being dragged, and then apply the path itinerary to the token.
        public override bool TryFulfilPromise(Token token, Context context)
        {
            
            if (!Visible)
                return false; //if the ghost isn't active, there's no promise to fulfill.
            
            var ghostPathItinerary = GetItineraryForFulfilment(token);

            ghostPathItinerary.Depart(token, new Context(Context.ActionSource.Unknown));

            
            return true;
        }

        public override TokenItinerary GetItineraryForFulfilment(Token token)
        {
            TokenPathItinerary pathToGhost =
                new TokenPathItinerary(token.TokenRectTransform.anchoredPosition3D, transform.position)
                    .WithDestinationSpherePath(_projectedInSphere.GetWildPath());

            return pathToGhost;
        }


    }



}
