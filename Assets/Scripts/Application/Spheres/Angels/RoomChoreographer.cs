using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Galaxy.Api;
using SecretHistories.Choreographers;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Angels
{
 //place tokens only where a Floor surface is present in the room

    
    public class RoomChoreographer: AbstractChoreographer
    {
        [SerializeField]
        private List<Transform> Floors;

        public override void PlaceTokenAtFreeLocalPosition(Token token, Context context)
        {
            var defaultPosition = Vector3.zero;
            token.TokenRectTransform.anchoredPosition3D = GetClosestFreeLocalPosition(token, defaultPosition);
        }

        public override void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 targetPosition)
        {

            Vector2 closestPosition = GetClosestFreeLocalPosition(token, targetPosition);

            Vector3 finalPositionAtTableLevel = new Vector3(closestPosition.x, closestPosition.y, Sphere.transform.position.z);
            token.TokenRectTransform.localPosition = finalPositionAtTableLevel;
        }

        public override LegalPositionCheckResult IsLegalPlacement(Rect candidateRect, Token placingToken)
        {
            return LegalPositionCheckResult.Legal();
        }

        public override Vector2 GetClosestFreeLocalPosition(Token token, Vector2 startPositionLocal)
        {
            float difference = float.PositiveInfinity;
            if (!Floors.Any())
            {
                NoonUtility.LogWarning($"No walkable floors in {Sphere.name}; defaulting to zero vector");
                return Vector2.zero;

            }

            Transform closestFloorLevel = null;

            foreach (var f in Floors)
            {
                var floorY = f.localPosition.y;
                if (Math.Abs(floorY - startPositionLocal.y) < difference)
                {
                    difference = Math.Abs(floorY - startPositionLocal.y);
                    closestFloorLevel = f;
                }
            }

            if (closestFloorLevel == null)
            {
                NoonUtility.LogWarning(
                    $"Couldn't decide a closest floor level in {Sphere.name}; defaulting to zero vector");
                return Vector2.zero;
            }
            else
            {

                var positionAtFloorLevel = new Vector2(startPositionLocal.x, closestFloorLevel.localPosition.y);
                
                return positionAtFloorLevel;
            }

        }
    }
}
