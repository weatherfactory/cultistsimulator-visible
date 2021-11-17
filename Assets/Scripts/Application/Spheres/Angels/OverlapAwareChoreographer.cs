using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Choreographers;
using SecretHistories.Entities;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Angels
{
    //make sure there's a position for incoming tokens, bearing in mind other itineraries.
    //once they arrive, place them assertively.
    //Assume all tokens are the same dimensions.
   public class OverlapAwareChoreographer: IChoreographer
    {
        private Sphere _sphere;
    private string _sphereToWatchForOverlap;

        public void Awake()
        {

        }

        public OverlapAwareChoreographer(Sphere sphere)
        {
            _sphere = sphere;
        }
        public void PlaceTokenAtFreeLocalPosition(Token token, Context context)
        {
            token.TokenRectTransform.anchoredPosition3D = Vector3.zero;
        }



        public void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 targetPosition)
        {
            token.TokenRectTransform.anchoredPosition3D = targetPosition;

        }

        public LegalPositionCheckResult IsLegalPlacement(Rect candidateRect, Token placingToken)
        {
            var overlapSphereToWatch = Watchman.Get<HornedAxe>().GetDefaultSphere();
            return overlapSphereToWatch.Choreographer.IsLegalPlacement(candidateRect,placingToken));
        }

        public Vector2 GetFreeLocalPosition(Token token, Vector2 startPos)
        {
            
            float sphereWidth = _sphere.GetRectTransform().rect.width;
            float halfSphereWidth = sphereWidth / 2; //as x, this would be our centre position
            float tokenWidth= token.ManifestationRectTransform.rect.width;
            float halfTokenWidth = tokenWidth/ 2; //we should offset the token at least half its manifestation's width to the right from the starting position, on the assumption it has a centre pivot
            //nb this is the manifestation's width, not the token's, because the token's may not have been updated at this point, and manifestation is our best guess

            float startingX = -halfSphereWidth + halfTokenWidth;
            float startingY = 0f;
            var tokensAlreadyPresent = _sphere.Tokens;
            float totalOffsetToRight = 0f;
            foreach (var t in tokensAlreadyPresent)
            {
                totalOffsetToRight += tokenWidth;//assuming all tokens are the same size
            }

            foreach (var i in Watchman.Get<Xamanek>().CurrentItinerariesForPath(_sphere.GetAbsolutePath()))
            {
                totalOffsetToRight += tokenWidth;
            }

            Vector2 nextPosition =new Vector2(startingX + totalOffsetToRight, startingY);
            

            return nextPosition;

        }

        public Vector3 SnapToGrid(Vector3 transformLocalPosition)
        {
            return Vector3.zero;
        }
    }
}
