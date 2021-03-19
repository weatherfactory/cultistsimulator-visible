using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Angels
{
   public class HorizontalLayoutChoreographer: IChoreographer
    {
        private Sphere _sphere;

        public HorizontalLayoutChoreographer(Sphere sphere)
        {
            _sphere = sphere;
        }
        public void PlaceTokenAtFreeLocalPosition(Token token, Context context)
        {
            token.TokenRectTransform.anchoredPosition3D = Vector3.zero;
        }

        public void PlaceTokenAssertivelyAtSpecifiedLocalPosition(Token token, Context context, Vector2 pos)
        {
            token.TokenRectTransform.anchoredPosition3D = pos;
        }

        public void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 pos)
        {
            var freeLocalPosition = GetFreeLocalPosition(token, pos);
            token.TokenRectTransform.anchoredPosition = freeLocalPosition;

        }

        public Vector2 GetFreeLocalPosition(Token token, Vector2 centerPos, int startIteration = -1)
        {
            
            float sphereWidth = _sphere.GetRectTransform().rect.width;
            float halfSphereWidth = sphereWidth / 2; //as x, this would be our centre position
            float halfTokenWidth = token.ManifestationRectTransform.rect.width / 2; //we should offset the token at least half its manifestation's width to the right from the starting position, on the assumption it has a centre pivot
            //nb this is the manifestation's width, not the token's, because the token's may not have been updated at this point, and manifestation is our best guess

            float startingX = -halfSphereWidth + halfTokenWidth;
            float startingY = 0f;
            var tokensAlreadyPresent = _sphere.Tokens;
            float totalTokenWidth = 0f;
            foreach (var t in tokensAlreadyPresent)
            {
                totalTokenWidth += t.TokenRectTransform.rect.width;
            }

            Vector2 nextPosition=new Vector2(startingX + totalTokenWidth, startingY);
            return nextPosition;

        }

        public Vector3 SnapToGrid(Vector3 transformLocalPosition)
        {
            return Vector3.zero;
        }
    }
}
