using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;


using UnityEngine;
using Random = System.Random;

namespace SecretHistories.Spheres.Angels
{
    public class GreedyAngel:IAngel
    {
        private const int GRAB_QUANTITY_LIMIT = 1;
        private const int BEATS_BETWEEN_ANGELRY = 20; 
        private int _beatsTowardsAngelry = 0;
        private ThresholdSphere _thresholdSphereToGrabTo;
        


        public int Authority => 9;

        public void Act(float seconds, float metaseconds)
        {
            _beatsTowardsAngelry++;

            if (_beatsTowardsAngelry >= BEATS_BETWEEN_ANGELRY)
           
                _beatsTowardsAngelry = 0;

            if (_thresholdSphereToGrabTo.Tokens
                .Any()) //default greedy angels will ignore spheres that already contain a token. Redundant with a check for validation, but stops us wasting time looking for tokens if we're already full!
                return;

            var alreadyIncoming = Watchman.Get<Xamanek>().GetCurrentItinerariesForPath(_thresholdSphereToGrabTo.GetWildPath());
            if (alreadyIncoming.Any()) //greedy angels will ignore spheres that already have a token en route to them
                return;

            
            TryGrabStack(_thresholdSphereToGrabTo, metaseconds);
            
        }

        public void SetWatch(Sphere thresholdSphereToGrabTo)
        {
            _thresholdSphereToGrabTo = thresholdSphereToGrabTo as ThresholdSphere;
     
            thresholdSphereToGrabTo.ShowAngelPresence(this);

        }

        public bool MinisterToDepartingToken(Token token, Context context)
        {
            return false; //could check here whether we're allowed to remove this token.
        }

        public bool MinisterToEvictedToken(Token token,Context context)
        {
            return false; // if  we want to grab tokens after they're evictedl this would be a good place
        }

        public void Retire()
        {
            _thresholdSphereToGrabTo.HideAngelPresence(this);
            Defunct = true;
        }
        public bool Defunct { get; protected set; }

        public void ShowRelevantVisibleCharacteristic(List<VisibleCharacteristic> visibleCharacteristics)
        {
            var greedyV =
                visibleCharacteristics.Where(v => v.VisibleCharacteristicId == VisibleCharacteristicId.Greedy);

            foreach (var v in greedyV)
                v.Show();
        }

        public void HideRelevantVisibleCharacteristic(List<VisibleCharacteristic> visibleCharacteristics)
        {
            var greedyV =
                visibleCharacteristics.Where(v => v.VisibleCharacteristicId == VisibleCharacteristicId.Greedy);

            foreach (var v in greedyV)
                v.Hide();
        }

        private void TryGrabStack(Sphere destinationThresholdSphere, float interval)
        {

            var spheresWhichAllowDragging = Watchman.Get<HornedAxe>().GetSpheresWhichAllowDragging();

            var worldSpheres = spheresWhichAllowDragging.Where(s => s.SphereCategory == SphereCategory.World);
            var outputSpheres = spheresWhichAllowDragging.Where(s => s.SphereCategory == SphereCategory.Output);
            var thresholdSpheres = spheresWhichAllowDragging.Where(s => s.SphereCategory == SphereCategory.Threshold);
            //at time of writing, these are the only three categories which allow dragging. In theory, there could be others! check here first if we see a greedy angel failing
            //but there could be more complex criteria anyway in the future, eg 'within range'
            //Also see below

            var spheresToSearch = new List<Sphere>();
            spheresToSearch.AddRange(worldSpheres);
            spheresToSearch.AddRange(outputSpheres);
            spheresToSearch.AddRange(thresholdSpheres);


            foreach (var sphereToSearch in spheresToSearch)
            {
                var matchingToken = FindStackForSlotSpecificationInSphere(destinationThresholdSphere.GoverningSphereSpec, sphereToSearch);

                //if (!_thresholdSphereToGrabTo.IsValidDestinationForToken(matchingToken))
                //    return;

                if (matchingToken != null)
                {
                    
                    if (matchingToken.CurrentlyBeingDragged()) //This shouldn't currently ever happen, because the EnRouteSphere doesn't allow dragging, but it might cover race conditions
                        matchingToken.ForceEndDrag();

                    if (matchingToken.Quantity > GRAB_QUANTITY_LIMIT)
                        matchingToken.CalveToken(matchingToken.Quantity - GRAB_QUANTITY_LIMIT,
                            new Context(Context.ActionSource.GreedyGrab));

                    TokenTravelItinerary itinerary = destinationThresholdSphere.GetItineraryFor(matchingToken).
                        WithDuration(NoonConstants.SEND_STACK_TO_SLOT_DURATION);

                    itinerary.Depart(matchingToken,new Context(Context.ActionSource.GreedyGrab));

                    return;
                }
            }
        }




        private Token FindStackForSlotSpecificationInSphere(SphereSpec slotSpec, Sphere sphereToSearch)
        {
            var rnd = new Random();
            var tokens = sphereToSearch.GetElementTokens().OrderBy(x => rnd.Next());

            foreach (var token in tokens)
                if (token.CanBePulled() && slotSpec.CheckPayloadAllowedHere(token.Payload).MatchType == SlotMatchForAspectsType.Okay)
                    return token;

            return null;
        }

    }
}
