using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    //places, arranges and displays things on the table
    public class Choreographer
    {
        private Tabletop _tabletop;
        private SituationBuilder _situationBuilder;
        private Rect tableRect;

        public Choreographer(Tabletop tabletop, SituationBuilder situationBuilder, Transform tableLevelTransform, Transform WindowLevelTransform)
        {
            _tabletop = tabletop;
            _situationBuilder = situationBuilder;

            tableRect = tabletop.GetRect();
            Debug.Log("Tabletop Rect is " + tableRect);
        }

        // -- POSITIONING ----------------------------

        const float checkPointPerArcLength = 70f;
        const float radiusBase = 50f;
        const float radiusIncrement = 35f;
        const float radiusMaxSize = 250f;

        Rect targetRect;
        bool tokenOverlaps;
        Vector2[] currentPoints;
        Rect finalRect;
        List<Rect> checkedRects;

        public void ArrangeTokenOnTable(SituationToken token) {
            token.GetRectTransform().anchoredPosition = GetFreeTokenPositionWithDebug(token, Vector2.zero);

            _tabletop.DisplaySituationTokenOnTable(token);
        }

        public void ArrangeTokenOnTable(ElementStackToken stack) {
            _tabletop.GetElementStacksManager().AcceptStack(stack); // this does parenting. Needs to happen before we position

            if (stack.lastTablePos != null) { 
                stack.GetRectTransform().anchoredPosition = GetFreeTokenPositionWithDebug(stack, stack.lastTablePos.Value);
            }
            else {
                stack.GetRectTransform().anchoredPosition = GetFreeTokenPositionWithDebug(stack, Vector2.zero);
                stack.lastTablePos = stack.GetRectTransform().anchoredPosition;
            }

            stack.transform.localRotation = Quaternion.identity;
            stack.DisplayAtTableLevel();
            stack.FlipToFaceUp(true);            
        }

        public Vector2 GetFreeTokenPositionWithDebug(DraggableToken token, Vector2 centerPos, Rect[] avoidPositions = null) {
            currentPoints = null;
            tokenOverlaps = false;
            checkedRects = new List<Rect>();

            var finalPos = GetFreeTokenPosition(token, centerPos, avoidPositions);

            if (!tokenOverlaps)
                return finalPos;

            finalRect = GetCenterPosRect(finalPos, token.GetRectTransform().rect.size);
            
            var debugInfo = new GameObject("ChoreoDebugInfo").AddComponent<ChoreographerDebugView>();
            debugInfo.Init(targetRect, tokenOverlaps, currentPoints, finalRect);
            debugInfo.tabletop = _tabletop.transform;
            debugInfo.checkedRects = new List<Rect>();

            foreach (var item in avoidPositions)
                checkedRects.Add(item);

            foreach (var item in _tabletop.GetTokenTransformWrapper().GetTokens())
                if (item != null && item.GetRectTransform() != null)
                    debugInfo.checkedRects.Add(GetCenterPosRect(item.GetRectTransform().anchoredPosition, item.GetRectTransform().rect.size));

            return finalPos;
        }


        public Vector2 GetFreeTokenPosition(DraggableToken token, Vector2 centerPos, Rect[] avoidPositions = null) {
            centerPos = GetPosClampedToTable(centerPos);
            targetRect = GetCenterPosRect(centerPos, token.GetRectTransform().rect.size);

            if (IsLegalPosition(targetRect, avoidPositions)) 
                return centerPos;

            Debug.Log("Position is occupied! Checking nearby.");

            tokenOverlaps = true;
            float radius = radiusBase;
            
            while (radius < radiusMaxSize) {
                currentPoints = GetTestPoints(targetRect.position + targetRect.size / 2f, radius);

                foreach (var point in currentPoints) {
                    if (IsLegalPosition(GetCenterPosRect(point, targetRect.size), avoidPositions)) 
                        return point;
                }

                radius += radiusIncrement;
            }

            Debug.Log("No Free Token position found! Rechecking all points with increased tolerance.");

            if (IsLegalPosition(GetCenterPosRect(targetRect.position, targetRect.size / 3f), avoidPositions))
                return centerPos;

            while (radius < radiusMaxSize) {
                currentPoints = GetTestPoints(targetRect.position + targetRect.size / 2f, radius);

                foreach (var point in currentPoints) {
                    if (IsLegalPosition(GetCenterPosRect(point, targetRect.size / 3f), avoidPositions))
                        return point;
                }

                radius += radiusIncrement;
            }

            Debug.LogWarning("No position found! Dumping in middle.");

            return Vector2.zero;
        }

        // Tokens have their pos in their center, rects in the bottom right
        Rect GetCenterPosRect(Vector2 centerPos, Vector2 size) {
            return new Rect(centerPos - size / 2f, size);
        }
                
        Vector2 GetPosClampedToTable(Vector2 pos) {
            const float padding = .2f;

            pos.x = Mathf.Clamp(pos.x, tableRect.x + padding, tableRect.x + tableRect.width - padding);
            pos.y = Mathf.Clamp(pos.y, tableRect.y + padding, tableRect.y + tableRect.height - padding);
            return pos;
        }

        bool IsLegalPosition(Rect rect, Rect[] avoidPositions = null) {
            if (tableRect.Contains(rect.position + rect.size / 2f) == false)
                return false;

            if (avoidPositions != null)
                foreach (var item in avoidPositions) 
                    if (item.Overlaps(rect)) 
                        return false;

            foreach (var item in _tabletop.GetTokenTransformWrapper().GetTokens()) 
                if (GetCenterPosRect(item.GetRectTransform().anchoredPosition, item.GetRectTransform().rect.size).Overlaps(rect))
                    return false;

            return true;
        }

        Vector2[] GetTestPoints(Vector3 pos, float radius) {
            float circumference = 2f * Mathf.PI * radius;
            int numPoints = Mathf.FloorToInt(circumference / checkPointPerArcLength);

            var points = new Vector2[numPoints];
            float angleSteps = Mathf.Deg2Rad * 360f / points.Length;

            for (int i = 0; i < points.Length; i++)
                points[i] = GetPointOnCircle(pos, radius, -i * angleSteps);

            return points;
        }

        Vector2 GetPointOnCircle(Vector3 origin, float radius, float angle) {
            return new Vector2(origin.x + radius * Mathf.Cos(angle),
                               origin.y + radius * Mathf.Sin(angle));
        }

        private bool TokenOverlapsPosition(DraggableToken token, Vector2 marginPixels, Vector2 candidatePos) {
            foreach (var t in _tabletop.GetTokenTransformWrapper().GetTokens()) {
                if (token != t
                    && candidatePos.x - t.transform.localPosition.x < marginPixels.x
                    && candidatePos.x - t.transform.localPosition.x > -marginPixels.x
                    && candidatePos.y - t.transform.localPosition.y < marginPixels.y
                    && candidatePos.y - t.transform.localPosition.y > -marginPixels.y) {
                    return true;
                }
            }

            return false;
        }

        // -- SITUATION MANAGEMENT ----------------------------

        public void BeginNewSituation(SituationCreationCommand scc)
        {
            if (scc.Recipe == null)
                throw new ApplicationException("DON'T PASS AROUND SITUATIONCREATIONCOMMANDS WITH RECIPE NULL");
            //if new situation is beginning with an existing verb: do not action the creation.

            //oh: I could have an scc property which is a MUST CREATE override
           
            var existingSituation = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations()
                .SingleOrDefault(sc => sc.situationToken.Id == scc.Recipe.ActionId);
            
            //grabbing existingtoken: just in case some day I want to, e.g., add additional tokens to an ongoing one rather than silently fail the attempt.
            if (existingSituation != null)
            {
                NoonUtility.Log("Tried to create " + scc.Recipe.Id + " for verb " + scc.Recipe.ActionId + " but that verb is already active.");
                //end execution here
                return;
            }
            var token = _situationBuilder.CreateTokenWithAttachedControllerAndSituation(scc);

            //if token has been spawned from an existing token, animate its appearance
            if (scc.SourceToken != null)
            {
                var tokenAnim = token.gameObject.AddComponent<TokenAnimation>();
                tokenAnim.onAnimDone += SituationAnimDone;
                tokenAnim.SetPositions(scc.SourceToken.RectTransform.anchoredPosition3D,
                    Registry.Retrieve<Choreographer>().
                        GetFreeTokenPosition(token, new Vector2(0, -250f)));
                tokenAnim.SetScaling(0f, 1f);
                tokenAnim.StartAnim();
            }
            else
            {
                Registry.Retrieve<Choreographer>().ArrangeTokenOnTable(token);
            }

        }

       public void MoveElementToSituationSlot(ElementStackToken stack, TokenAndSlot tokenSlotPair)
        {
            var stackAnim = stack.gameObject.AddComponent<TokenAnimationToSlot>();
            stackAnim.onElementSlotAnimDone += ElementGreedyAnimDone;
            stackAnim.SetPositions(stack.RectTransform.anchoredPosition3D, tokenSlotPair.Token.GetOngoingSlotPosition());
            stackAnim.SetScaling(1f, 0.35f);
            stackAnim.SetTargetSlot(tokenSlotPair);
            stackAnim.StartAnim(0.2f);
        }

        void ElementGreedyAnimDone(ElementStackToken element, TokenAndSlot tokenSlotPair)
        {
            if (!tokenSlotPair.RecipeSlot.Equals(null))
                tokenSlotPair.RecipeSlot.AcceptStack(element);
        }

        void SituationAnimDone(SituationToken token)
        {
            _tabletop.DisplaySituationTokenOnTable(token);
        }

    }
}
