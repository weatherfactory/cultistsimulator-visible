using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;


namespace SecretHistories.Spheres.Angels
{
    //low-aggression angel which tidies up evicted tokens with no particular home
    public class TidyAngel: IAngel
    {
        private FucinePath _tidyToPath;
        private readonly string _tidyPayloadsOfType;

        public int Authority => 0;

        public TidyAngel(FucinePath tidyToPath,string tidyPayloadsOfType)
        {
            _tidyToPath = tidyToPath;
            _tidyPayloadsOfType = tidyPayloadsOfType;
        }

        public void Act(float seconds, float metaseconds)
        {
            //do nothing?
        }

        public void SetWatch(Sphere sphere)
        {
            _tidyToPath = sphere.GetAbsolutePath();
        }

        public bool MinisterToDepartingToken(Token token, Context context)
        {
            return true; //if it's been purposefully removed, it's not our business
        }

        public bool MinisterToEvictedToken(Token token, Context context)
        {
            if(token.PayloadTypeName!= _tidyPayloadsOfType)
                return false; 

            var tidyToSphere = Watchman.Get<HornedAxe>().GetSphereByPath(_tidyToPath);

            if (!tidyToSphere.IsInRangeOf(token.Sphere))
                return false;

            TokenTravelItinerary travellingToTidyTarget =
                new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D,
                        tidyToSphere.Choreographer.GetFreeLocalPosition(token, Vector3.zero))
                    .WithDestinationSpherePath(tidyToSphere.GetAbsolutePath())
                    .WithDuration(NoonConstants.MOMENT_TIME_INTERVAL);

            travellingToTidyTarget.Depart(token,context);


            return true;
        }

        public void Retire()
        {
            Defunct = true;
        }

        public bool Defunct { get; protected set; }
        public void ShowRelevantVisibleCharacteristic(List<VisibleCharacteristic> visibleCharacteristics)
        {
            //
        }

        public void HideRelevantVisibleCharacteristic(List<VisibleCharacteristic> visibleCharacteristics)
        {
            //
        }
    }
}
