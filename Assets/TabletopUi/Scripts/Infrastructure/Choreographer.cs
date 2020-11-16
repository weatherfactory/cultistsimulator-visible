using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts.TokenContainers;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure {
    //places, arranges and displays things on the table
    public class Choreographer:ISettingSubscriber {

        private Sphere _tabletop;
        private Rect tableRect;

        const float checkPointPerArcLength = 100f;

        const float pointGridSize = 100f;
        private float gridSnapSize = 0.0f;
        const int maxGridIterations = 5;

        const float radiusBase = 50f;
        const float radiusIncrement = 50f;
        const float radiusMaxSize = 250f;

        private ChoreographerDebugView _currentDebug;

        public Choreographer(Sphere sphere) {
            _tabletop = sphere;


            tableRect = _tabletop.GetRect();

            var snapGridSetting = Registry.Get<ICompendium>().GetEntityById<Setting>(NoonConstants.GRIDSNAPSIZE);
            if (snapGridSetting != null)
            {
                snapGridSetting.AddSubscriber(this);
                WhenSettingUpdated(snapGridSetting.CurrentValue);
            }
            else
                NoonUtility.Log("Missing setting entity: " + NoonConstants.GRIDSNAPSIZE);
        }


     public void PlaceTokenOnTableAtFreePosition(Token token, Context context) {
            
            
            token.RectTransform.anchoredPosition = GetFreePosWithDebug(token, Vector2.zero);
            _tabletop.DisplayHere(token, context);
        }


/// <summary>
/// Place at a specific position, pushing other tokens out of the way if necessary
/// </summary>
            public void PlaceTokenAssertivelyAtSpecifiedPosition(Token token, Context context, Vector2 pos)
		{
            _tabletop.AcceptToken(token, context);  // this does parenting. Needs to happen before we position


            token.rectTransform.anchoredPosition = pos;
            token.LastTablePos = pos;
            token.transform.localRotation = Quaternion.identity;
     
			token.SnapToGrid();

            MoveAllTokensOverlappingWith(token);

        }


/// <summary>
/// Place as close to a specific position as we can get
/// </summary>
        public void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 pos)
{
    token.RectTransform.anchoredPosition = GetFreePosWithDebug(token, pos);
    _tabletop.DisplayHere(token, context);

        }

        #region -- POSITIONING HELP METHODS ----------------------------

        public void MoveAllTokensOverlappingWith(Token pushingToken)
		{
			if (pushingToken.NoPush)
			{
				return;
			}

            var targetRect = GetCenterPosRect(pushingToken.RectTransform);
			// Reduce the target Rect size to be less finnicky
			targetRect.size = targetRect.size * 0.6f;

			Rect pushedRect;

            foreach (var token in _tabletop.GetAllTokens()) {
                if (CanTokenBeIgnored(token, pushingToken))
                    continue;

				pushedRect = GetCenterPosRect(token.rectTransform);

				if (!pushedRect.Overlaps(targetRect))
                    continue;

                token.AnimateTo( 0.2f,
                    token.rectTransform.anchoredPosition3D,
                    GetFreePosWithDebug(token, token.rectTransform.anchoredPosition),
                    null,
                    1f,
                    1f);
            }
        }

        // Note: If we're using the world pos while an object is dragged we get the wrong pos, since we project from cam through the raised pos
        public Vector2 GetTablePosForWorldPos(Vector3 worldPos) {
            Vector2 localPoint;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_tabletop.transform as RectTransform, screenPoint, Camera.main, out localPoint);

            return localPoint;
        }

        #endregion

        #region -- GET FREE POSITION ----------------------------

        public Vector2 GetFreePosWithDebug(Token token, Vector2 centerPos, int startIteration = -1)
		{
#if DEBUG
            _currentDebug = new GameObject("ChoreoDebugInfo_" + token.name).AddComponent<ChoreographerDebugView>();
            _currentDebug.tabletop = _tabletop.transform;
            _currentDebug.targetRect = GetCenterPosRect(centerPos, token.RectTransform.rect.size);
            _currentDebug.checkedPoints = new List<Vector2>();
            _currentDebug.tokenOverlaps = false;
            _currentDebug.checkedRects = new List<Rect>();

            var pos = GetFreeTokenPosition(token, centerPos, startIteration);
            _currentDebug.finalRect = GetCenterPosRect(pos, token.RectTransform.rect.size);
            _currentDebug.hasDebugData = true;

            _currentDebug.InitKill(10f); // In 10s, debug thing kills itself

            return pos;
#else
            return GetFreeTokenPosition(token, centerPos, startIteration);
#endif
        }

        Vector2 GetFreeTokenPosition(Token token, Vector2 centerPos, int startIteration = -1)
		{
            //Debug.Log("Trying to find FREE POS for " + token.Id);
            centerPos = GetPosClampedToTable(centerPos);
			centerPos = SnapToGrid( centerPos );
			var targetRect = GetCenterPosRect(centerPos, token.RectTransform.rect.size);

            if (IsLegalPosition(targetRect, token))
                return centerPos;

            if (_currentDebug != null)
			{
                _currentDebug.targetRect = targetRect;
                _currentDebug.tokenOverlaps = true;
			}

            // We grab a bunch of test points
            startIteration = startIteration > 0f ? startIteration : 1;
            var currentPoints = GetTestPoints(targetRect.position + targetRect.size / 2f, startIteration, maxGridIterations);

            // Go over the test points and check if there's a clear spot to place things
            foreach (var point in currentPoints)
			{
                if (IsLegalPosition(GetCenterPosRect(point, targetRect.size), token))
                    return point;
            }

            // we've exhausted things, so we do again
            // but we  shift the target pos a bit
            centerPos = centerPos + targetRect.size;
            // and we reduce the smaller rect size. This allows overlap
            targetRect.size = targetRect.size * 0.4f;

            //Debug.Log("Did not find a legal pos for " + token.Id + ", allowing for overlap!");

            // request a new set of points, since the center pos has shifted
            currentPoints = GetTestPoints(targetRect.position + targetRect.size / 2f, startIteration, maxGridIterations);

            if (IsLegalPosition(GetCenterPosRect(targetRect.position, targetRect.size), token))
                return centerPos;

            foreach (var point in currentPoints)
			{
                if (IsLegalPosition(GetCenterPosRect(point, targetRect.size), token))
                    return point;
            }

            NoonUtility.Log("Choreographer: No legal tabletop position found for " + token.Element.Id + " (" + centerPos + ")!",1);

            return Vector2.zero;
        }

        // Tokens have their pos in their center, rects in the bottom right
        Rect GetCenterPosRect(RectTransform rectTrans)
		{
            return GetCenterPosRect(rectTrans.anchoredPosition, rectTrans.rect.size);
        }

        Rect GetCenterPosRect(Vector2 centerPos, Vector2 size)
		{
            return new Rect(centerPos - size / 2f, size);
        }

        Vector2 GetPosClampedToTable(Vector2 pos)
		{
            const float padding = .2f;

            pos.x = Mathf.Clamp(pos.x, tableRect.x + padding, tableRect.x + tableRect.width - padding);
            pos.y = Mathf.Clamp(pos.y, tableRect.y + padding, tableRect.y + tableRect.height - padding);
            return pos;
        }

        bool IsLegalPosition(Rect rect,  Token ignoreToken = null)
		{
            if (tableRect.Contains(rect.position + rect.size / 2f) == false)
                return false;
            
            Rect rectCheck;
            //Debug.Log("Checking if " + rect + " is a legal position");

            foreach (var token in _tabletop.GetAllTokens()) {
                rectCheck = GetCenterPosRect(token.rectTransform);

                if (CanTokenBeIgnored(token, ignoreToken))
                    continue;

                if (_currentDebug != null && !_currentDebug.checkedRects.Contains(rectCheck)) { 
                    _currentDebug.checkedRects.Add(rectCheck);
                    //Debug.Log("Checking for " + token.name + " at " + rectCheck);
                }

                if (rectCheck.Overlaps(rect)) {
                    //Debug.Log("Not a legal pos");
                    return false;
                }
            }

            //Debug.Log("IS a legal pos");
            return true;
        }

        bool CanTokenBeIgnored(Token token, Token ignoreToken) {
            if (token == ignoreToken)
                return true;
            if (token.IsInMotion)
                return true;
            if (token.Defunct)
                return true;
			if (token.NoPush)
				return true;

            return false;
        }

        Vector2[] GetTestPoints(Vector3 pos, int startIteration, int maxIteration)
		{
            int numPoints = 0;
			// Always test in half-card intervals for best chance of finding valid slot
			float snap_x = 90.0f * 0.5f;
			float snap_y = 130.0f * 0.5f;

            for (int i = startIteration; i <= maxIteration; i++)
			{
                numPoints += 8 * i;
            }

            var points = new Vector2[numPoints];

            int p = 0;
            float y;
            float x;
            
            for (int v = 1 ; v < 2 + maxIteration * 2; v++)
			{
                y = (v % 2 == 0 ? -(v / 2) : (v / 2));

                for (int h = 1; h < 2 + maxIteration * 2; h++)
				{
                    if (h <= -1 + startIteration * 2 && v <= -1 + startIteration * 2)
                        continue; // don't put out points lower than our startIteration


					x = (h % 2 == 0 ? (h / 2) : -(h / 2));


                    points[p] = new Vector2(pos.x + x * snap_x, pos.y + y * snap_y);
					points[p] = SnapToGrid( points[p] );

                    if (_currentDebug != null) 
                        _currentDebug.checkedPoints.Add(points[p]);

                    p++;
                }
            }

            return points;
        }

        /*

        Vector2[] GetTestPoints(Vector3 pos, int gridIteration) {
            var points = new Vector2[8 * gridIteration];

            int p = 0;

            // right side points
            for (int i = -gridIteration; i <= gridIteration; i++) {
                points[p] = new Vector2(pos.x + gridIteration * pointGridSize, pos.y + i * pointGridSize);
                p++;
            }

            // left side points
            for (int i = -gridIteration; i <= gridIteration; i++) {
                points[p] = new Vector2(pos.x + -gridIteration * pointGridSize, pos.y + i * pointGridSize);
                p++;
            }

            // top side points
            for (int i = -gridIteration + 1; i <= gridIteration - 1; i++) {
                points[p] = new Vector2(pos.x + i * pointGridSize, pos.y + gridIteration * pointGridSize);
                p++;
            }

            // bottom side points
            for (int i = -gridIteration + 1; i <= gridIteration - 1; i++) {
                points[p] = new Vector2(pos.x + i * pointGridSize, pos.y + -gridIteration * pointGridSize);
                p++;
            }

            return points;
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
        */

        #endregion



        public void SetGridSnapSize(float snapsize)
        {
            int snap = Mathf.RoundToInt(snapsize);
            switch (snap)
            {
                default:
                case 0: gridSnapSize = 0.0f; break;
                case 1: gridSnapSize = 1.0f; break;     // 1 card
                case 2: gridSnapSize = 0.5f; break;     // ½ card
                case 3: gridSnapSize = 0.25f; break;    // ¼ card
            }
        }

        public float GetGridSnapSize()
        {
            return gridSnapSize;
        }



        public Vector3 SnapToGrid(Vector3 v)
        {
            if (GetGridSnapSize() > 0f)
            {
                // Magical maths to snap cards to fractions of approx card dimensions - CP
                float snap_x = 90.0f * GetGridSnapSize();
                float snap_y = 130.0f * GetGridSnapSize();
                float recip_x = 1.0f / snap_x;
                float recip_y = 1.0f / snap_y;
                v.x *= recip_x; v.x = (float)Mathf.RoundToInt(v.x); v.x *= snap_x;
                v.y *= recip_y; v.y = (float)Mathf.RoundToInt(v.y); v.y *= snap_y;
            }
            return v;
        }

        public void WhenSettingUpdated(object newValue)
        {
            SetGridSnapSize(newValue is float ? (float)newValue : 0);
        }




    }
}
