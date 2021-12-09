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
        private Sphere _tidyToSphere;
        private readonly string _tidyPayloadsOfType;

        public int Authority => 0;

        /// <summary>
        /// We used to use tidytopath - but that breaks things if the path changes because, eg, we are dragging a dropzone sphere through EnRoute
        /// </summary>
        /// <param name="tidyToSphere"></param>
        /// <param name="tidyPayloadsOfType"></param>
        public TidyAngel(Sphere tidyToSphere,string tidyPayloadsOfType)
        {
            _tidyToSphere=tidyToSphere;
            _tidyPayloadsOfType = tidyPayloadsOfType;
        }

        public void Act(float seconds, float metaseconds)
        {
            //do nothing?
        }

        public void SetWatch(Sphere sphere)
        {
            _tidyToSphere = sphere;

        }

        public bool MinisterToDepartingToken(Token token, Context context)
        {
            return true; //if it's been purposefully removed, it's not our business
        }

        public bool MinisterToEvictedToken(Token token, Context context)
        {
            if(token.PayloadTypeName!= _tidyPayloadsOfType)
                return false; 

 
            if (!_tidyToSphere.IsInRangeOf(token.Sphere))
                return false;

            TokenTravelItinerary travellingToTidyTarget =
                new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D,
                        _tidyToSphere.Choreographer.GetFreeLocalPosition(token, Vector3.zero))
                    .WithDestinationSpherePath(_tidyToSphere.GetAbsolutePath())
                    .WithDuration(NoonConstants.MOMENT_TIME_INTERVAL);

            travellingToTidyTarget.Depart(token,context);


            return true;
        }

        public void Retire()
        {
            Defunct = true;
        }


        public bool Defunct { get; protected set; }
        public bool RequestingRetirement { get; private set; }
        public void RequestRetirement()
        {
            RequestingRetirement = true;
        }

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
