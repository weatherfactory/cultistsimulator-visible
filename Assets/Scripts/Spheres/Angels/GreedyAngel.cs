﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.Scripts.States.TokenStates;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.Spheres.Angels
{
    public class GreedyAngel:IAngel
    {
        private const int BEATS_BETWEEN_ANGELRY = 20; 
        private int _beatsTowardsAngelry = 0;

        public void MinisterTo(Sphere sphere,float interval)
        {

            _beatsTowardsAngelry++;

            if (_beatsTowardsAngelry >= BEATS_BETWEEN_ANGELRY)
           
                _beatsTowardsAngelry = 0;

            if (!sphere.CurrentlyBlockedFor(BlockDirection.Inward) && sphere.GetAllTokens().Count == 0)
                TryGrabStack(sphere, interval);
            
        }

        private void TryGrabStack(Sphere destinationThresholdSphere, float interval)
        {

            var worldSpheres = Registry.Get<SphereCatalogue>().GetSpheresOfCategory(SphereCategory.World);
            foreach (var worldSphereToSearch in worldSpheres)
            {
                var matchingToken = FindStackForSlotSpecificationInSphere(destinationThresholdSphere.GoverningSlotSpecification, worldSphereToSearch);
                if (matchingToken != null)
                {
                    
                    if (matchingToken.CurrentlyBeingDragged())
                        matchingToken.FinishDrag();

                    var enRouteSphere = Registry.Get<SphereCatalogue>().GetDefaultEnRouteSphere();

                    var targetPosition = GetTargetPositionForDestinationSphere(destinationThresholdSphere);

                    TokenTravelItinerary itinerary = new TokenTravelItinerary(matchingToken.Location.Position,
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
            var targetPosition = targetSituation.GetAnchorLocation().Position;
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
