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
        private readonly SphereBlock _angelBlock = new SphereBlock(BlockDirection.Inward, BlockReason.GreedyAngel);


        public int Authority => 9;

        public void Act(float seconds, float metaseconds)
        {
            _beatsTowardsAngelry++;

            if (_beatsTowardsAngelry >= BEATS_BETWEEN_ANGELRY)
           
                _beatsTowardsAngelry = 0;

            if (!_thresholdSphereToGrabTo.CurrentlyBlockedForDirectionWithAnyReasonExcept(BlockDirection.Inward,BlockReason.GreedyAngel) && _thresholdSphereToGrabTo.Tokens.Count == 0)
                TryGrabStack(_thresholdSphereToGrabTo, metaseconds);
            
        }

        public void SetWatch(Sphere thresholdSphereToGrabTo)
        {
            _thresholdSphereToGrabTo = thresholdSphereToGrabTo as ThresholdSphere;
     
            thresholdSphereToGrabTo.ShowAngelPresence(this);
            _thresholdSphereToGrabTo.AddBlock(_angelBlock);
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
            _thresholdSphereToGrabTo.RemoveBlock(_angelBlock);
            _thresholdSphereToGrabTo.HideAngelPresence(this);
            Defunct = true;
        }

        public bool Defunct { get; protected set; }
        public bool RequestingRetirement { get; }

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

  
            foreach (var sphereToSearch in spheresWhichAllowDragging)
            {
                var matchingToken = FindStackForSlotSpecificationInSphere(destinationThresholdSphere.GoverningSphereSpec, sphereToSearch);
                if (matchingToken != null)
                {
                    
                    if (matchingToken.CurrentlyBeingDragged())
                        matchingToken.FinishDrag();

                    if (matchingToken.Quantity > GRAB_QUANTITY_LIMIT)
                        matchingToken.CalveToken(matchingToken.Quantity - GRAB_QUANTITY_LIMIT,
                            new Context(Context.ActionSource.GreedyGrab));


                    TokenTravelItinerary itinerary = destinationThresholdSphere.GetItineraryFor(matchingToken).WithDuration(NoonConstants.SEND_STACK_TO_SLOT_DURATION);

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
