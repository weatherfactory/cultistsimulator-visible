using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using SecretHistories.Choreographers;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Angels
{
    //Assume all tokens are the same dimensions and place them in a row.
    //skip gaps created by tokens in the world sphere.
    public class WorldAwareRowChoreographer: RowChoreographer
    {
        public override LegalPositionCheckResult IsLegalPlacement(Rect candidateRect, Token placingToken)
        {
            Sphere overlapSphereToWatch;
            FucinePath fpOverlapSphereToWatch = new FucinePath(PathOfOverlapSphereToWatch);
            if (!fpOverlapSphereToWatch.IsValid())
                overlapSphereToWatch = Watchman.Get<HornedAxe>().GetDefaultSphere();
            else
                overlapSphereToWatch = Watchman.Get<HornedAxe>().GetSphereByPath(fpOverlapSphereToWatch);
            var result = LegalInThisAndOtherRelevantSpheres(overlapSphereToWatch, candidateRect, placingToken);


            return result;
        }


        private LegalPositionCheckResult LegalInThisAndOtherRelevantSpheres(Sphere overlapSphere, Rect candidateRect,
            Token placingToken)
        {
            Rect otherTokenOverlapRect;

            foreach (var otherToken in Sphere.Tokens.Where(t => t.PayloadId != placingToken.PayloadId))
            {
                if (!CanTokenBeIgnored(otherToken))
                {
                    otherTokenOverlapRect =
                        otherToken
                            .GetRectInCurrentSphere(); //we need the token's rect in the current sphere, not in the world sphere, to compare with the candidate rect we've just calculated for current sphere
                    if (UnacceptableOverlap(otherTokenOverlapRect, candidateRect, GetGridSnapCoefficient()))

                        return LegalPositionCheckResult.Blocked(otherToken.name, otherTokenOverlapRect);
                }
            }

            foreach (var otherToken in overlapSphere.Tokens)
            {
                if (!CanTokenBeIgnored(otherToken))
                {
                    otherTokenOverlapRect =
                        otherToken.GetRectInOtherSphere(
                            Sphere); //we need the token's rect in the current sphere, not in the world sphere, to compare with the candidate rect we've just calculated for current sphere
                    if (UnacceptableOverlap(otherTokenOverlapRect, candidateRect, GetGridSnapCoefficient()))

                        return LegalPositionCheckResult.Blocked(otherToken.name, otherTokenOverlapRect);
                }
            }

            return LegalPositionCheckResult.Legal();

        }
    }

}

