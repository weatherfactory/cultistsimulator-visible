using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;
using SecretHistories.States.TokenStates;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Interfaces;

using UnityEngine;
using Random = System.Random;

namespace SecretHistories.Spheres.Angels
{
    public class GreedyAngel:IAngel
    {
        private const int BEATS_BETWEEN_ANGELRY = 20; 
        private int _beatsTowardsAngelry = 0;
        private Sphere _thresholdSphereToGrabTo;
        private readonly HashSet<Sphere> _spheresToGrabFrom=new HashSet<Sphere>();

        public void SetThresholdToGrabTo(Sphere thresholdSphereToGrabTo)
        {
            _thresholdSphereToGrabTo = thresholdSphereToGrabTo;
        }

        public void Act(float interval)
        {
            _beatsTowardsAngelry++;

            if (_beatsTowardsAngelry >= BEATS_BETWEEN_ANGELRY)
           
                _beatsTowardsAngelry = 0;

            if (!_thresholdSphereToGrabTo.CurrentlyBlockedFor(BlockDirection.Inward) && _thresholdSphereToGrabTo.GetAllTokens().Count == 0)
                TryGrabStack(_thresholdSphereToGrabTo, interval);
            
        }

        public void SetWatch(Sphere sphere)
        {
            _spheresToGrabFrom.Add(sphere);
        }

        private void TryGrabStack(Sphere destinationThresholdSphere, float interval)
        {
  
            foreach (var sphereToSearch in _spheresToGrabFrom)
            {
                var matchingToken = FindStackForSlotSpecificationInSphere(destinationThresholdSphere.GoverningSlotSpecification, sphereToSearch);
                if (matchingToken != null)
                {
                    
                    if (matchingToken.CurrentlyBeingDragged())
                        matchingToken.FinishDrag();

                    var enRouteSphere = Registry.Get<SphereCatalogue>().GetDefaultEnRouteSphere();

                    var targetPosition = GetTargetPositionForDestinationSphere(destinationThresholdSphere);

                    TokenTravelItinerary itinerary = new TokenTravelItinerary(matchingToken.Location.Anchored3DPosition,
                            targetPosition)
                        .WithScaling(1f,0.35f)
                        .WithSphereRoute(enRouteSphere, destinationThresholdSphere);

                    
                    itinerary.Depart(matchingToken);

                    return;
                }
            }
        }

        private Vector3 GetTargetPositionForDestinationSphere(Sphere destinationThresholdSphere)
        {
            //if we're using this for non-classic-CS-recipe slots, we'll need to rewrite it. We'll also need not to hardcode the final scale
            var targetPath = destinationThresholdSphere.GetPath();
            var targetSituationPath = targetPath.SituationPath;
            var targetSituation = Registry.Get<SituationsCatalogue>().GetSituationByPath(targetSituationPath);
            var targetPosition = targetSituation.GetAnchorLocation().Anchored3DPosition;
            return targetPosition;
        }


        private Token FindStackForSlotSpecificationInSphere(SlotSpecification slotSpec, Sphere sphereToSearch)
        {
            var rnd = new Random();
            var tokens = sphereToSearch.GetElementTokens().OrderBy(x => rnd.Next());

            foreach (var token in tokens)
                if (token.CanBePulled() && slotSpec.GetSlotMatchForAspects(token.ElementStack.GetAspects()).MatchType == SlotMatchForAspectsType.Okay)
                    return token;

            return null;
        }

    }
}
