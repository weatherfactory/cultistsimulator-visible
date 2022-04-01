using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Abstract;
using SecretHistories.Choreographers;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Angels
{
 //place tokens only where a Floor surface is present in the room

    
    public class RoomChoreographer: AbstractChoreographer
    {

        private float GRID_WIDTH=20f;

        private float GRID_HEIGHT=20f;
        private List<WalkableFloor> _floors;
        private List<WalkableLadder> _ladders;

        public List<WalkableFloor> GetWalkableFloors()
        {
            return new List<WalkableFloor>(_floors);

        }

        public List<WalkableLadder> GetWalkableLadders()
        {
            return new List<WalkableLadder>(_ladders);

        }

        public void Awake()
        {
            _floors = GetComponentsInChildren<WalkableFloor>().ToList();
            _ladders = GetComponentsInChildren<WalkableLadder>().ToList();
        }

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
       
            Rect otherTokenOverlapRect;

            foreach (var otherToken in Sphere.Tokens.Where(t => t != placingToken && t.OccupiesSameSpaceAs(placingToken) && !CanTokenBeIgnored(t)))
            {
                otherTokenOverlapRect = otherToken.GetRectInCurrentSphere();

                var overlapModifier = 1f;

 
                if (UnacceptableOverlap(otherTokenOverlapRect, candidateRect, overlapModifier))

                    return LegalPositionCheckResult.Blocked(otherToken.name, otherTokenOverlapRect);

            }

            return LegalPositionCheckResult.Legal();
        }

        public override Vector2 GetClosestFreeLocalPosition(Token token, Vector2 startPositionLocal)
        {
            var closestWalkablePosition= GetClosestWalkablePosition(token, startPositionLocal);
            
            var targetRect = token.GetRectFromPosition(closestWalkablePosition);
            var legalPositionCheckResult = IsLegalPlacement(targetRect, token);
            if (legalPositionCheckResult.IsLegal)
                return closestWalkablePosition;
                

            Vector2 direction = (closestWalkablePosition - legalPositionCheckResult.BlockerRect.center).normalized;
            var testRects = GetAlternativeCandidateRectsAlongVector(targetRect, direction, 1, 100, GRID_WIDTH, GRID_HEIGHT);
            foreach (var testRect in testRects)
            {
                if (IsLegalPlacement(testRect, token).IsLegal)
                    return testRect.center;
            }
            
            NoonUtility.Log(
                $"Choreographer: No legal walkable position found for {token.name})! Just putting it at zero", 1);
            
            return Vector2.zero;
        }

        private Vector2 GetClosestWalkablePosition(Token token, Vector2 startPositionLocal)
        {
       
            if (!_floors.Any() && !_ladders.Any())
            {
                NoonUtility.LogWarning($"No walkable floors or ladders in {Sphere.name}; defaulting to zero vector");
                return Vector2.zero;
            }


            WalkableFloor closestFloor = null;
            //var tokenHeightAdjustment = token.ManifestationRectTransform.rect.height *0.65f;
            var tokenHeightAdjustment = token.ManifestationRectTransform.rect.height * 0f;


            float flooryDifference = float.PositiveInfinity;
            foreach (var f in _floors)
            {
                var floorY = f.gameObject.transform.localPosition.y;
                if (Math.Abs(floorY - startPositionLocal.y) < flooryDifference)
                {
                    flooryDifference = Math.Abs(floorY - startPositionLocal.y);
                    closestFloor = f;
                }
            }

            WalkableLadder closestLadder = null;
            float ladderXDifference = float.PositiveInfinity;
            foreach (var l in _ladders)
            {
                var ladderX = l.gameObject.transform.localPosition.x;
                if (Math.Abs(ladderX - startPositionLocal.x) < ladderXDifference)
                {
                    ladderXDifference = Math.Abs(ladderX - startPositionLocal.x);
                    closestLadder = l;
                }
            }


            if (closestLadder != null && ladderXDifference < flooryDifference)
            {
                var positionOnLadder =
                    new Vector2(closestLadder.gameObject.transform.localPosition.x,
                        startPositionLocal.y + tokenHeightAdjustment);
                return positionOnLadder;
            }
            else if
                (closestFloor != null &&
                 flooryDifference <
                 float.PositiveInfinity) //second condition is redundant, but keeping it here in case of accident
            {
                var positionAtFloorLevel = new Vector2(startPositionLocal.x,
                    closestFloor.gameObject.transform.localPosition.y + tokenHeightAdjustment);
                return positionAtFloorLevel;
            }
            else
            {
                NoonUtility.LogWarning(
                    $"Couldn't decide a closest walkable surface in {Sphere.name}; defaulting to zero vector");
                return Vector2.zero;
            }
        }
    }
}
