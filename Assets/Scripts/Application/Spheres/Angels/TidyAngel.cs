using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using UnityEngine;


namespace SecretHistories.Spheres.Angels
{
    //low-aggression angel which tidies up evicted tokens with no particular home
    public class TidyAngel: IAngel
    {
        private SpherePath _tidyToPath;

        public int Authority => 0;

        public TidyAngel(SpherePath tidyToPath)
        {
            _tidyToPath = tidyToPath;
        }

        public void Act(float interval)
        {
            //do nothing?
        }

        public void SetWatch(Sphere sphere)
        {
            _tidyToPath = sphere.GetPath();
        }

        public bool MinisterToEvictedToken(Token token, Context context)
        {

            var tidyToSphere = Registry.Get<SphereCatalogue>().GetSphereByPath(_tidyToPath);

            if (!tidyToSphere.IsInRangeOf(token.Sphere))
                return false;

            TokenTravelItinerary travellingHome =
                new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D,
                        tidyToSphere.Choreographer.GetFreeLocalPosition(token, Vector3.zero))
                    .WithSphereRoute(Registry.Get<SphereCatalogue>().GetDefaultEnRouteSphere(),
                        tidyToSphere)
                    .WithDuration(NoonConstants.MOMENT_TIME_INTERVAL);

            travellingHome.Depart(token,context);


            return true;
        }

    }
}
