using System;
using System.Collections.Generic;
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
    public class RowChoreographer : AbstractChoreographer
    {
 

            public int MaxPlacementAttempts { get; set; }
            public float InternalSpacing { get; set; }

            public string PathOfOverlapSphereToWatch;

            public override void PlaceTokenAtFreeLocalPosition(Token token, Context context)
            {
                var acceptablePosition = GetFreeLocalPosition(token, Vector3.zero);
                token.TokenRectTransform.anchoredPosition3D = acceptablePosition;
        }



            public override void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context,
                Vector2 targetPosition)
            {
                var acceptablePosition = GetFreeLocalPosition(token, targetPosition);
                token.TokenRectTransform.anchoredPosition3D = acceptablePosition;

            }

            public override LegalPositionCheckResult IsLegalPlacement(Rect candidateRect, Token placingToken)
            {
                Sphere overlapSphereToWatch;
                FucinePath fpOverlapSphereToWatch = new FucinePath(PathOfOverlapSphereToWatch);
                if (!fpOverlapSphereToWatch.IsValid())
                    overlapSphereToWatch = Watchman.Get<HornedAxe>().GetDefaultSphere();
                else
                    overlapSphereToWatch = Watchman.Get<HornedAxe>().GetSphereByPath(fpOverlapSphereToWatch);
                 var result = LegalInThisAndInOverlappingSphere(overlapSphereToWatch, candidateRect, placingToken);


                return result;
            }


            private LegalPositionCheckResult LegalInThisAndInOverlappingSphere(Sphere overlapSphere, Rect candidateRect,
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

            public override bool CanTokenBeIgnored(Token token)
            {

                IHasAspects dontWorryAbout = Sphere.GetContainer();

                if (dontWorryAbout.Id == token.PayloadId)
                    return true;

                return base.CanTokenBeIgnored(token);
            }

            public override Vector2 GetFreeLocalPosition(Token token, Vector2 startPos)
            {

                float sphereWidth = Sphere.GetRectTransform().rect.width;
                float halfSphereWidth = sphereWidth / 2; //as x, this would be our centre position
                float tokenWidth = token.ManifestationRectTransform.rect.width;
                float
                    halfTokenWidth =
                        tokenWidth /
                        2; //we should offset the token at least half its manifestation's width to the right from the starting position, on the assumption it has a centre pivot
                //nb this is the manifestation's width, not the token's, because the token's may not have been updated at this point, and manifestation is our best guess

                float startingX = -halfSphereWidth + halfTokenWidth;
                float startingY = 0f;
                Vector2 candidatePosition = new Vector2(startingX, startingY);



                //start at left, offset to right until we have no collisions either in the row sphere or in its world overlap
                int failedPlacementAttempts = 0;
                var placementIsLegal = IsLegalPlacement(token.GetRectFromPosition(candidatePosition), token);
                while (failedPlacementAttempts < MaxPlacementAttempts && !placementIsLegal.IsLegal)
                {
                    candidatePosition.x += tokenWidth + InternalSpacing;
                    failedPlacementAttempts++;
                    placementIsLegal = IsLegalPlacement(token.GetRectFromPosition(candidatePosition), token);
                }

                //offset once more for each incoming token
                foreach (var i in Watchman.Get<Xamanek>().GetCurrentItinerariesForPath(Sphere.GetAbsolutePath()))
                {
                    if (i.Key != token.PayloadId)
                        candidatePosition.x += tokenWidth + InternalSpacing;
                }

                return candidatePosition;

            }

        }
    
}
