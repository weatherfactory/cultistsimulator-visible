using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Abstract;
using SecretHistories.Choreographers;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Angels
{
 //place tokens only where a Floor surface is present in the room

    
    public class RoomChoreographer: AbstractChoreographer
    {

        private float GRID_WIDTH=5f;

        private float GRID_HEIGHT=5f;
        private List<WalkableFloor> _floors;
        private List<WalkableLadder> _ladders;
        private List<PileablesPlacementGuide> _ppGuides;


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
            _ppGuides = GetComponentsInChildren<PileablesPlacementGuide>().ToList();

        }

        public override void PlaceTokenAtFreeLocalPosition(Token token, Context context)
        {
            var defaultPosition = Vector3.zero;
            token.TokenRectTransform.anchoredPosition3D = GetClosestFreeLocalPosition(token, defaultPosition).Vector3;
        }

        public override void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 targetPosition)
        {

            Vector2 closestPosition = GetClosestFreeLocalPosition(token, targetPosition).Vector3;

            token.TokenRectTransform.anchoredPosition3D = closestPosition;
        }

        public override LegalPositionCheckResult IsLegalPlacement(Rect candidateRect, Token placingToken)
        {
            var roomRect = GetSphereRect();
            if (!roomRect.Overlaps(candidateRect))
                return LegalPositionCheckResult.Illegal();

            foreach (var otherToken in Sphere.Tokens.Where(t => t != placingToken && t.OccupiesSameSpaceAs(placingToken) && !CanTokenBeIgnored(t)))
            {
                var otherTokenOverlapRect = otherToken.GetRectInCurrentSphere();

                var overlapModifier = 1f;

 
                if (UnacceptableOverlap(otherTokenOverlapRect, candidateRect, overlapModifier))

                    return LegalPositionCheckResult.Blocked(otherToken.name, otherTokenOverlapRect);

            }

            return LegalPositionCheckResult.Legal();
        }

        public override ChoreoPosition GetClosestFreeLocalPosition(Token token, Vector2 startPositionLocal)
        {
            if (token.OccupiesSpaceAs() == OccupiesSpaceAs.PhysicalObject || token.OccupiesSpaceAs() == OccupiesSpaceAs.LargePhysicalObject)
            {
                return ClosestLegalPileablePositionFor(token, startPositionLocal);
            }

            return ClosestLegalWalkablePositionFor(token, startPositionLocal);
        }

        private ChoreoPosition ClosestLegalWalkablePositionFor(Token token, Vector2 startPositionLocal)
        {
            var closestWalkablePosition = GetClosestPositionOnAWalkableSurface(token, startPositionLocal);

            var targetRect = token.GetRectFromPosition(closestWalkablePosition);
            var legalPositionCheckResult = IsLegalPlacement(targetRect, token);
            if (legalPositionCheckResult.IsLegal)
                return new ChoreoPosition(closestWalkablePosition);


            Vector2 direction;
            //not legal. Which direction are we looking in? NB this is bobbins atm
            if (startPositionLocal.x <= legalPositionCheckResult.BlockerRect.x)
                direction = Vector2.left;
            else
                direction = Vector2.right;


            var testRects = GetAlternativeCandidateRectsAlongVector(targetRect, direction, 1, 50, GRID_WIDTH, GRID_HEIGHT);
            foreach (var testRect in testRects)
            {
                if (IsLegalPlacement(testRect, token).IsLegal)
                    return new ChoreoPosition(testRect.position);
            }

            //if we can't find any test rects before the end, reverse direction and try the other way.
            var alternateTestRects =
                GetAlternativeCandidateRectsAlongVector(targetRect, -direction, 1, 50, GRID_WIDTH, GRID_HEIGHT);
            foreach (var altTestRect in alternateTestRects)
            {
                if (IsLegalPlacement(altTestRect, token).IsLegal)
                    return new ChoreoPosition( altTestRect.position);
            }

            NoonUtility.Log(
                $"Choreographer: No legal walkable position found for {token.name})! Returning null", 1);

            return null;
        }

        private ChoreoPosition ClosestLegalPileablePositionFor(Token token, Vector2 startPositionLocal)
        {

            var closestWalkablePosition = GetClosestPositionOnAPPGuide(token, startPositionLocal);
            var targetRect = token.GetRectFromPosition(closestWalkablePosition);
            var legalPositionCheckResult = IsLegalPlacement(targetRect, token);
            if (legalPositionCheckResult.IsLegal)
                return new ChoreoPosition(closestWalkablePosition);
     
            if(legalPositionCheckResult.Legality == PositionLegality.OutOfBounds)
            {
                NoonUtility.Log(
                    $"Choreographer: Position sought for {token.name}) was out of bounds! Returning null", 1);

                return null;
            }

            //we have a blocker. Look for positions on top until we reach the max height, and if we find none look left and right.

            int maxItemsInPile = 5; //for example;
            
            float placingTokenHeight = targetRect.height;

            var candidatePosition = closestWalkablePosition;

            //start at left, offset to right until we have no collisions either in the row sphere or in its world overlap
            int failedPlacementAttempts = 0;
            var placementIsLegal = IsLegalPlacement(token.GetRectFromPosition(candidatePosition), token);
            while (failedPlacementAttempts < maxItemsInPile && !placementIsLegal.IsLegal)
            {
                candidatePosition.y += placingTokenHeight;
                failedPlacementAttempts++;
                placementIsLegal = IsLegalPlacement(token.GetRectFromPosition(candidatePosition), token);
            }


            if (placementIsLegal.IsLegal)
                return new ChoreoPosition(candidatePosition);

            else
            {
                //giving up. Which isn't a long-term solution
                return null;
            }
            

        }

        private Vector3 GetClosestPositionOnAPPGuide(Token token, Vector2 startPositionLocal)
        {
            
            if (!_ppGuides.Any())
            {
                NoonUtility.LogWarning($"No walkable ppguides in {Sphere.name}; defaulting to zero vector");
                return Vector3.zero;
            }


            PileablesPlacementGuide closestPPG = null;

            float ppgYDifference = float.PositiveInfinity;
            foreach (var f in _ppGuides)
            {
                if (!f.TokenAllowedHere(token))
                    continue;
                var ppGuideY = f.gameObject.transform.localPosition.y;
                if (Math.Abs(ppGuideY - startPositionLocal.y) < ppgYDifference)
                {
                    ppgYDifference = Math.Abs(ppGuideY - startPositionLocal.y);
                    closestPPG = f;
                }
            }

            if
                (closestPPG != null &&
                 ppgYDifference <
                 float.PositiveInfinity) //second condition is redundant, but keeping it here in case of accident
            {
                var positionAtPPGuideLevel = new Vector2(startPositionLocal.x,
                    closestPPG.anchorY);
                return positionAtPPGuideLevel;
            }
            else
            {
                NoonUtility.LogWarning(
                    $"Couldn't decide a closest ppg surface in {Sphere.name}; defaulting to zero vector");
                return Vector3.zero; ;
            }

        }

        private Vector3 GetClosestPositionOnAWalkableSurface(Token token, Vector2 startPositionLocal)
        {

            if (!_floors.Any() && !_ladders.Any())
            {
                NoonUtility.LogWarning($"No walkable floors or ladders in {Sphere.name}; defaulting to zero vector");
                return Vector3.zero;
            }


            WalkableFloor closestFloor = null;
            
            float flooryDifference = float.PositiveInfinity;
            foreach (var f in _floors)
            {
                if (!f.TokenAllowedHere(token))
                    continue;
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
                if(!l.TokenAllowedHere(token))
                    continue;
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
                    new Vector2(closestLadder.anchorX,
                        startPositionLocal.y);
                return positionOnLadder;
            }
            else if
                (closestFloor != null &&
                 flooryDifference <
                 float.PositiveInfinity) //second condition is redundant, but keeping it here in case of accident
            {
                var positionAtFloorLevel = new Vector2(startPositionLocal.x,
                    closestFloor.anchorY);
                return positionAtFloorLevel;
            }
            else
            {
                NoonUtility.LogWarning(
                    $"Couldn't decide a closest walkable surface in {Sphere.name}; defaulting to zero vector");
                return Vector3.zero;
            }
        }
    }
}
