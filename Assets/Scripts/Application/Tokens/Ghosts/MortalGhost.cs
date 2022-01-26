using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Ghosts;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Tokens.Ghosts
{
    public class MortalGhost: AbstractGhost
    {
        public override bool TryFulfilPromise(Token token, Context context)
        {
            if (!Visible)
                return false; //if the ghost isn't active, there's no promise to fulfill.

            //otherwise, we did show the ghost, so we'd better be ready to make good on it.
            TokenTravelItinerary travellingToGhost =
                new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D, rectTransform.anchoredPosition3D)
                    .WithDuration(10f)
                    .WithDestinationSpherePath(_projectedInSphere.GetWildPath());

            travellingToGhost.Depart(token, context);

            HideIn(token); //now clean up the ghost

            //and say that we've fulfilled the promise
            return true;
        }
    }
}
