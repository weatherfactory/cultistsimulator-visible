using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.Spheres.Angels
{
    public class GreedyAngel:IAngel
    {
        public void MinisterTo(Sphere sphere)
        {
            if (sphere.GetAllTokens().Any())
                return;

            var worldSpheres = Registry.Get<SphereCatalogue>().GetSpheresOfCategory(SphereCategory.World);
            foreach (var worldSphereToSearch in worldSpheres)
            {
                var matchingToken = FindStackForSlotSpecificationInSphere(sphere.GoverningSlotSpecification,worldSphereToSearch);
                if (matchingToken != null)
                {
                    NoonUtility.Log("This is where the angel for " + sphere.GetPath() +" would pull " + matchingToken.name);

                    if (matchingToken.CurrentlyBeingDragged)
                    {
                        matchingToken.SetXNess(TokenXNess.DivertedByGreedySlot);
                        matchingToken.FinishDrag();
                    }

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
