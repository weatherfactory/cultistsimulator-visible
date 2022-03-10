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
        private List<Sphere> _spheresForAutostack = new List<Sphere>();
        private readonly string _tidyPayloadsOfType;

        public int Authority => 0;

        /// <summary>
        /// We used to use tidytopath - but that breaks things if the path changes because, eg, we are dragging a dropzone sphere through EnRoute
        /// </summary>
        /// <param name="tidyPayloadsOfType"></param>
        public TidyAngel(string tidyPayloadsOfType)
        {
            _tidyPayloadsOfType = tidyPayloadsOfType;
        }

        public void Act(float seconds, float metaseconds)
        {
            //do nothing?
        }

        public void SetWatch(Sphere sphere)
        {
            TidyToSphere(sphere);
        }

        public void TidyToSphere(Sphere sphere)
        {
            //optional. If a TidyAngel is watching over a sphere, it'll move Evicted tokens to that sphere after trying to autostack them.
            _tidyToSphere = sphere;
        }

        public bool AddSphereForAutostack(Sphere sphere)
        {
            if (_spheresForAutostack.Contains(sphere))
                return false;
            
            _spheresForAutostack.Add(sphere);
                return true;
        }

        public bool RemoveSphereForAutostack(Sphere sphere)
        {
            if (!_spheresForAutostack.Contains(sphere))
                return false;

            _spheresForAutostack.Remove(sphere);
                return true;
        }

        public bool MinisterToDepartingToken(Token token, Context context)
        {
            return true; //if it's been purposefully removed, it's not our business
        }

        public bool MinisterToEvictedToken(Token evictedToken, Context context)
        {
            //first, check all our autostack spheres. Are there any tokens we should directly merge with?
            //This kind of merge is instant, no animation.

            if (evictedToken.PayloadTypeName != _tidyPayloadsOfType) //Only tidy eg cards or tiles.
                return false;

            foreach (var autostacksphere in _spheresForAutostack)
            {
                var tokensForPotentialAutostack =autostacksphere.GetTokens();

                foreach (var a in tokensForPotentialAutostack)
                {
                    if (evictedToken.Payload.CanMergeWith(a.Payload))
                    {
                        a.Payload.InteractWithIncoming(evictedToken);
                        return true;
                    }
                }
            }


            //secondly, if we have a tidytosphere, move the tokens somewhere suitable there.
            //NB that the tidytosphere may be one of the autostack spheres - in which case autostack supersedes the tidy animation


            if (_tidyToSphere!=null)
            {
         
                if (!_tidyToSphere.IsInRangeOf(evictedToken.Sphere))
                    return false;

                TokenTravelItinerary travellingToTidyTarget =
                    new TokenTravelItinerary(evictedToken.TokenRectTransform.anchoredPosition3D,
                            _tidyToSphere.Choreographer.GetClosestFreeLocalPosition(evictedToken, Vector3.zero))
                        .WithDestinationSpherePath(_tidyToSphere.GetAbsolutePath())
                        .WithDuration(NoonConstants.MOMENT_TIME_INTERVAL);

                travellingToTidyTarget.Depart(evictedToken,context);


                return true;
            }
 

            return false;
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
