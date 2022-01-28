using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Entities;
using SecretHistories.Ghosts;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Tokens.Ghosts
{
    public class MortalGhost: AbstractGhost
    {

        private bool activeAsPathBeacon=false; //TEMPORARY! deactivate when complete or it'll only work once

        ///Get a path itinerary based on the ghost's current position. Return the token to its
        /// original location if it's being dragged, and then apply the path itinerary to the token.
        public override bool TryFulfilPromise(Token token, Context context)
        {
           

            if (!Visible)
                return false; //if the ghost isn't active, there's no promise to fulfill.
            if (activeAsPathBeacon)
                return false;

            //otherwise, we did show the ghost, so we'd better be ready to make good on it.
            var travelToGhostItinerary = GetItineraryForFulfilment(token);
            activeAsPathBeacon = true;
            token.FinishDrag(); //This should remove the token from its being-dragged state (and generally return it to its origin point before it begins moving on its path)
            //originally FinishDrag() hid the ghost, too, but ofc we don't want to hide it for a path.
            //(another way round would be to make path-indicating ghosts behave differently from potential-indicating ghosts.
            
            travelToGhostItinerary.Depart(token, context);

            //TODO: hide the ghost once the token arrives and/or when we lose focus (in which case show it again when we get the focus back?)

            //and say that we've fulfilled the promise
            return true;
        }

        public override TokenItinerary GetItineraryForFulfilment(Token token)
        {
            //TokenPathItinerary pathToGhost =
            //    new TokenPathItinerary(token.TokenRectTransform.anchoredPosition3D, rectTransform.anchoredPosition3D)
            //        .WithDestinationSpherePath(_projectedInSphere.GetWildPath())

            TokenTravelItinerary travellingToGhost =
                new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D, rectTransform.anchoredPosition3D)
                    .WithDuration(5f)
                    .WithDestinationSpherePath(_projectedInSphere.GetWildPath());

            return travellingToGhost;
        }
    }



}
