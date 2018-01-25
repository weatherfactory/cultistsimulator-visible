﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure {
    //places, arranges and displays things on the table
    public class Choreographer {
        private TabletopTokenContainer _tabletop;
        private SituationBuilder _situationBuilder;
        private Rect tableRect;

        public Choreographer(TabletopTokenContainer tabletop, SituationBuilder situationBuilder, Transform tableLevelTransform, Transform WindowLevelTransform) {
            _tabletop = tabletop;
            _situationBuilder = situationBuilder;

            tableRect = tabletop.GetRect();
            Debug.Log("Tabletop Rect is " + tableRect);
        }

        // -- POSITIONING ----------------------------

        const float checkPointPerArcLength = 100f;
        const float radiusBase = 50f;
        const float radiusIncrement = 35f;
        const float radiusMaxSize = 250f;

        ChoreographerDebugView currentDebug;

        public void ArrangeTokenOnTable(SituationToken token) {
            token.RectTransform.anchoredPosition = GetFreePosWithDebug(token, Vector2.zero);

            _tabletop.DisplaySituationTokenOnTable(token);
        }

        public void ArrangeTokenOnTable(ElementStackToken stack) {
            _tabletop.GetElementStacksManager().AcceptStack(stack); // this does parenting. Needs to happen before we position

            if (stack.lastTablePos != null) {
                stack.RectTransform.anchoredPosition = GetFreePosWithDebug(stack, stack.lastTablePos.Value);
            }
            else {
                stack.RectTransform.anchoredPosition = GetFreePosWithDebug(stack, Vector2.zero);
                stack.lastTablePos = stack.RectTransform.anchoredPosition;
            }

            stack.transform.localRotation = Quaternion.identity;
            stack.DisplayAtTableLevel();
            stack.FlipToFaceUp(true);
        }

        public void MoveAllTokensOverlappingWith(DraggableToken token) {
            var targetRect = GetCenterPosRect(token.RectTransform);

            foreach (var item in _tabletop.GetTokens()) {
                if (item == token)
                    continue;
                if (!GetCenterPosRect(item.RectTransform).Overlaps(targetRect))
                    continue;

                AnimateTokenTo(item,
                    duration: 0.2f,
                    startPos: item.RectTransform.anchoredPosition3D,
                    endPos: GetFreePosWithDebug(item, item.RectTransform.anchoredPosition));
            }
        }

        public Vector2 GetFreePosWithDebug(DraggableToken token, Vector2 centerPos, float startRadius = -1f) {
            currentDebug = new GameObject("ChoreoDebugInfo_" + token.name).AddComponent<ChoreographerDebugView>();
            currentDebug.tabletop = _tabletop.transform;
            currentDebug.targetRect = GetCenterPosRect(centerPos, token.RectTransform.rect.size);
            currentDebug.checkedPoints = new List<Vector2>();
            currentDebug.tokenOverlaps = false;
            currentDebug.checkedRects = new List<Rect>();

            var pos = GetFreeTokenPosition(token, centerPos, startRadius);
            currentDebug.finalRect = GetCenterPosRect(pos, token.RectTransform.rect.size);
            currentDebug.hasDebugData = true;

            return pos;
        }

        public Vector2 GetFreeTokenPosition(DraggableToken token, Vector2 centerPos, float startRadius = -1f) {
            centerPos = GetPosClampedToTable(centerPos);
            var targetRect = GetCenterPosRect(centerPos, token.RectTransform.rect.size);

            if (IsLegalPosition(targetRect, token))
                return centerPos;

            if (currentDebug != null) {
                currentDebug.targetRect = targetRect;
                currentDebug.tokenOverlaps = true;
            }

            float radius = startRadius > 0f ? startRadius : radiusBase;

            while (radius <= radiusMaxSize) {
                var currentPoints = GetTestPoints(targetRect.position + targetRect.size / 2f, radius);

                foreach (var point in currentPoints) {
                    if (currentDebug != null)
                        currentDebug.checkedPoints.Add(point);

                    if (IsLegalPosition(GetCenterPosRect(point, targetRect.size), token))
                        return point;
                }

                radius += radiusIncrement;
            }

            if (IsLegalPosition(GetCenterPosRect(targetRect.position, targetRect.size / 3f), token))
                return centerPos;

            while (radius < radiusMaxSize) {
                var currentPoints = GetTestPoints(targetRect.position + targetRect.size / 2f, radius);

                foreach (var point in currentPoints) {
                    if (IsLegalPosition(GetCenterPosRect(point, targetRect.size / 3f), token))
                        return point;
                }

                radius += radiusIncrement;
            }

            return Vector2.zero;
        }

        // Tokens have their pos in their center, rects in the bottom right
        Rect GetCenterPosRect(RectTransform rectTrans) {
            return GetCenterPosRect(rectTrans.anchoredPosition, rectTrans.rect.size);
        }

        Rect GetCenterPosRect(Vector2 centerPos, Vector2 size) {
            return new Rect(centerPos - size / 2f, size);
        }

        Vector2 GetPosClampedToTable(Vector2 pos) {
            const float padding = .2f;

            pos.x = Mathf.Clamp(pos.x, tableRect.x + padding, tableRect.x + tableRect.width - padding);
            pos.y = Mathf.Clamp(pos.y, tableRect.y + padding, tableRect.y + tableRect.height - padding);
            return pos;
        }

        bool IsLegalPosition(Rect rect,  DraggableToken ignoreToken = null) {
            if (tableRect.Contains(rect.position + rect.size / 2f) == false)
                return false;
            
            Rect rectCheck;

            foreach (var item in _tabletop.GetTokens()) {
                rectCheck = GetCenterPosRect(item.RectTransform);

                if (item == ignoreToken || item.IsBeingAnimated)
                    continue;

                if (currentDebug != null && !currentDebug.checkedRects.Contains(rectCheck))
                    currentDebug.checkedRects.Add(rectCheck);

                if (rectCheck.Overlaps(rect))
                    return false;
            }

            return true;
        }

        Vector2[] GetTestPoints(Vector3 pos, float radius) {
            float circumference = 2f * Mathf.PI * radius;
            int numPoints = Mathf.FloorToInt(circumference / checkPointPerArcLength);
            int remainder = numPoints % 4;

            if (remainder != 0) // making sure we're always a mulitple of 4
                numPoints += 4 - remainder;

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

        // -- SITUATION MANAGEMENT ----------------------------

        public void BeginNewSituation(SituationCreationCommand scc) {
            if (scc.Recipe == null)
                throw new ApplicationException("DON'T PASS AROUND SITUATIONCREATIONCOMMANDS WITH RECIPE NULL");

            //if new situation is beginning with an existing verb: do not action the creation.
            //oh: I could have an scc property which is a MUST CREATE override

            var registeredSits = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();
            var existingSituation = registeredSits.SingleOrDefault(sc => sc.situationToken.Id == scc.Recipe.ActionId);

            //grabbing existingtoken: just in case some day I want to, e.g., add additional tokens to an ongoing one rather than silently fail the attempt.
            if (existingSituation != null) {
                NoonUtility.Log("Tried to create " + scc.Recipe.Id + " for verb " + scc.Recipe.ActionId + " but that verb is already active.");
                return;
            }

            var token = _situationBuilder.CreateTokenWithAttachedControllerAndSituation(scc);

            //if token has been spawned from an existing token, animate its appearance
            if (scc.SourceToken != null) {
                AnimateTokenTo(token,
                    duration: 1f,
                    startPos: scc.SourceToken.RectTransform.anchoredPosition3D,
                    endPos: GetFreePosWithDebug(token, scc.SourceToken.RectTransform.anchoredPosition, 200f),
                    startScale: 0f,
                    endScale: 1f);
            }
            else {
                Registry.Retrieve<Choreographer>().ArrangeTokenOnTable(token);
            }
        }

        void AnimateTokenTo(DraggableToken token, float duration, Vector3 startPos, Vector3 endPos, float startScale = 1f, float endScale = 1f) {
            var tokenAnim = token.gameObject.AddComponent<TokenAnimation>();
            tokenAnim.onAnimDone += SituationAnimDone;
            tokenAnim.SetPositions(startPos, endPos);
            tokenAnim.SetScaling(startScale, endScale);
            tokenAnim.StartAnim(duration);
        }

        public void MoveElementToSituationSlot(ElementStackToken stack, TokenAndSlot tokenSlotPair) {
            var stackAnim = stack.gameObject.AddComponent<TokenAnimationToSlot>();
            stackAnim.onElementSlotAnimDone += ElementGreedyAnimDone;
            stackAnim.SetPositions(stack.RectTransform.anchoredPosition3D, tokenSlotPair.Token.GetOngoingSlotPosition());
            stackAnim.SetScaling(1f, 0.35f);
            stackAnim.SetTargetSlot(tokenSlotPair);
            stackAnim.StartAnim(0.2f);
        }

        void ElementGreedyAnimDone(ElementStackToken element, TokenAndSlot tokenSlotPair) {
            if (!tokenSlotPair.RecipeSlot.Equals(null))
                tokenSlotPair.RecipeSlot.AcceptStack(element);
        }

        void SituationAnimDone(SituationToken token) {
            _tabletop.DisplaySituationTokenOnTable(token);
        }

    }
}
