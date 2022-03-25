using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Choreographers;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Angels
{
    public class ShelfChoreographer:AbstractChoreographer
    {
        public override void PlaceTokenAtFreeLocalPosition(Token token, Context context)
        {
            var acceptablePosition = GetClosestFreeLocalPosition(token, Vector3.zero);
            token.TokenRectTransform.anchoredPosition3D = acceptablePosition;
        }

        public override void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 targetPosition)
        {
            var acceptablePosition = GetClosestFreeLocalPosition(token, targetPosition);
            token.TokenRectTransform.anchoredPosition3D = acceptablePosition;
        }

        public override LegalPositionCheckResult IsLegalPlacement(Rect candidateRect, Token placingToken)
        {
            foreach (var otherToken in Sphere.Tokens.Where(t => t.PayloadId != placingToken.PayloadId))
            {
                if (!CanTokenBeIgnored(otherToken))
                {
                    var otherTokenOverlapRect = otherToken
                        .GetRectInCurrentSphere();

                    if (otherTokenOverlapRect.Overlaps(candidateRect))
                        return LegalPositionCheckResult.Blocked(otherToken.name, otherTokenOverlapRect);
                }
            }
            return LegalPositionCheckResult.Legal();

        }

        public override Vector2 GetClosestFreeLocalPosition(Token token, Vector2 originalIntendedPos)
        {
            float shelfWidth = Sphere.GetRectTransform().rect.width;
            float placingTokenWidth=token.TokenRectTransform.rect.width;
            float changeDirection = -10f; //not 0f, faint chance of loop
            float startX = 0f;
            float startY = 0f;
            int maxIterations = (int)(shelfWidth/placingTokenWidth);
            if (originalIntendedPos.x < 0)
            {
                changeDirection = 1f;
                startX = -(shelfWidth / 2);
                //startX+=  placingTokenWidth / 2; //can't do this until I stabilise the token width info
                //on the left side
            }
            else
            {
                changeDirection = -1f;
                startX = shelfWidth / 2;
                //startX-= placingTokenWidth/2 ;//can't do this until I stabilise the token width info
                //on the right side.
            }

            var candidatePosition = new Vector2(startX, startY);
         //   NoonUtility.Log($" x: { candidatePosition.x } ");
            var positionLegality = IsLegalPlacement(token.GetRectFromPosition(candidatePosition), token);
            if (positionLegality.IsLegal)
                return candidatePosition;

            //first choice isn't legal, let's go looking.

            for (int i = 0; i < maxIterations; i++)
            {
                float adjustment = (positionLegality.BlockerRect.width+1) * changeDirection;
                candidatePosition.x += adjustment;
                NoonUtility.Log($"adjustment: {adjustment} newX: {candidatePosition.x} ");
                positionLegality = IsLegalPlacement(token.GetRectFromPosition(candidatePosition), token);
                if (positionLegality.IsLegal)
                    return candidatePosition;
            }

            if (!positionLegality.IsLegal)
            {
                NoonUtility.Log($"Found nowhere to put token on shelf, even after {maxIterations} iterations");
            }

            return candidatePosition;
        }
    }
}
