using System;
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
                    
                    NoonUtility.Log("This is where the angel for " + destinationThresholdSphere.GetPath() + " would pull " + matchingToken.name);


                    if (matchingToken.CurrentlyBeingDragged())
                        matchingToken.FinishDrag();

                    var enRouteSphere = Registry.Get<SphereCatalogue>().GetDefaultEnRouteSphere();

                    TokenTravelItinerary itinerary = new TokenTravelItinerary(matchingToken.Location.Position,
                            destinationThresholdSphere.GetRectTransform().anchoredPosition3D)
                        .WithScaling(1f,1f)
                        .WithDuration(NoonConstants.SEND_STACK_TO_SLOT_DURATION)
                        .WithSphereRoute(enRouteSphere, destinationThresholdSphere);

                    destinationThresholdSphere.AddBlock(new ContainerBlock(BlockDirection.Inward,
                        BlockReason.InboundTravellingStack));

                    itinerary.Depart(matchingToken);

                    return;
                }
            }
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
