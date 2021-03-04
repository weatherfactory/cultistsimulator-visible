using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Entities.Verbs;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Spheres.Angels;
using SecretHistories.Services;
using SecretHistories.Spheres;


using UnityEngine;
using UnityEngine.InputSystem.HID;

namespace SecretHistories.Constants {
    //places, arranges and displays things on the table

    public class TabletopChoreographer:MonoBehaviour, ISettingSubscriber,IChoreographer {

       class LegalPositionCheckResult
       {
           public bool IsLegal=false;
           public string BlockerName;
           public Rect BlockerRect;


           public static LegalPositionCheckResult Legal()
           {
               return new LegalPositionCheckResult() { IsLegal = true};
           }

            public static LegalPositionCheckResult Illegal()
           {
               return new LegalPositionCheckResult();
            }

            public static LegalPositionCheckResult Blocked(string name,Rect rect)
            {
                return new LegalPositionCheckResult(){BlockerName = name,BlockerRect = rect};
            }
        }

        public TabletopSphere _tabletop;
        private Rect tableRect;

        const float checkPointPerArcLength = 100f;

        const float pointGridSize = 100f;
        private float gridSnapSize = 0.0f;
        const int maxGridIterations = 5;

        const float radiusBase = 50f;
        const float radiusIncrement = 50f;
        const float radiusMaxSize = 250f;

        private Dictionary<string,Rect> rectanglesToDisplay=new Dictionary<string, Rect>();

        public void Awake() {
            
            tableRect = _tabletop.GetRect();

            var snapGridSetting = Watchman.Get<Compendium>().GetEntityById<Setting>(NoonConstants.GRIDSNAPSIZE);
            if (snapGridSetting != null)
            {
                snapGridSetting.AddSubscriber(this);
                WhenSettingUpdated(snapGridSetting.CurrentValue);
            }
            else
                NoonUtility.Log("Missing setting entity: " + NoonConstants.GRIDSNAPSIZE);
        }


        public void OnGUI()
        {
          foreach (var r in rectanglesToDisplay)
                GUI.Box(r.Value, r.Key);
        }

        public void GroupAllStacks()
        {
            var stackTokens = _tabletop.GetElementTokens();
            var groups = stackTokens
                .GroupBy(e => e.Payload.GetSignature(), e => e)
                .Select(group => group.OrderByDescending(e => e.Payload.Quantity).ToList());

            foreach (var group in groups)
            {
                var primaryStackToken = group.First();

                foreach (var stackToken in group.Skip(1))
                    if (primaryStackToken.Payload.CanMergeWith(stackToken.Payload))
                    {
                        primaryStackToken.Payload.InteractWithIncoming(stackToken);
                    }
            }

        }

        public void PlaceTokenAtFreeLocalPosition(Token token, Context context)
     {
         token.TokenRectTransform.anchoredPosition = GetFreeLocalPosition(token, Vector2.zero);
     }


/// <summary>
/// Place at a specific position, pushing other tokens out of the way if necessary
/// </summary>
            public void PlaceTokenAssertivelyAtSpecifiedLocalPosition(Token token, Context context, Vector2 pos)
		{
            token.TokenRectTransform.anchoredPosition = pos;
            token.transform.localRotation = Quaternion.identity;

            SnapToGrid(token.transform.localPosition);

            MoveAllTokensOverlappingWith(token);

        }


/// <summary>
/// Place as close to a specific position as we can get
/// </summary>
        public void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 pos)
{
    var freeLocalPosition= GetFreeLocalPosition(token, pos);
    token.TokenRectTransform.anchoredPosition = freeLocalPosition;

}

public void MoveAllTokensOverlappingWith(Token pushingToken)
		{
			if (pushingToken.NoPush)
			{
				return;
			}

            var targetRect = GetRectWithSpherePosition(pushingToken.TokenRectTransform);
			// Reduce the target Rect size to be less finnicky
			targetRect.size = targetRect.size * 0.6f;

			Rect pushedRect;

            foreach (var token in _tabletop.Tokens) {
                if (token==pushingToken || CanTokenBeIgnored(token))
                    continue;

				pushedRect = GetRectWithSpherePosition(token.TokenRectTransform);

				if (!pushedRect.Overlaps(targetRect))
                    continue;

                TokenTravelItinerary itinerary=new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D, GetFreeLocalPosition(token, token.TokenRectTransform.anchoredPosition))
                    .WithDuration(0.2f)
                    .WithScaling(1f,1f);

                token.TravelTo(itinerary,new Context(Context.ActionSource.PushedAside));
            }
        }

        // Note: If we're using the world pos while an object is dragged we get the wrong pos, since we project from cam through the raised pos
        public Vector2 GetTablePosForWorldPos(Vector3 worldPos) {
            Vector2 localPoint;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_tabletop.transform as RectTransform, screenPoint, Camera.main, out localPoint);

            return localPoint;
        }

        public Vector2 GetFreeLocalPosition(Token token, Vector2 centerPos, int startIteration = -1)
		{
            var pos = GetFreeTokenPosition(token, centerPos, startIteration);

            return pos;
        }

        Vector2 GetFreeTokenPosition(Token token, Vector2 centerPos, int startIteration = -1)
		{
            //Debug.Log("Trying to find FREE POS for " + token.Id);
            centerPos = GetPosClampedToTable(centerPos);
			centerPos = SnapToGrid( centerPos );
			var targetRect = GetCenterPosRect(centerPos, token.ManifestationRectTransform.rect.size);

            var legalPositionCheckResult = IsLegalPosition(targetRect, token);
            if (legalPositionCheckResult.IsLegal)
            {
                HideAllRects();
                return centerPos;
            }
            else
            {
                ShowRect(targetRect, token.name);
                ShowRect(legalPositionCheckResult.BlockerRect, legalPositionCheckResult.BlockerName);
            }

        
        

            // We grab a bunch of test points
            startIteration = startIteration > 0f ? startIteration : 1;
            var currentPoints = GetTestPoints(targetRect.position + targetRect.size / 2f, startIteration, maxGridIterations);

            // Go over the test points and check if there's a clear spot to place things
            foreach (var point in currentPoints)
			{
                if (IsLegalPosition(GetCenterPosRect(point, targetRect.size), token).IsLegal)
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

            if (IsLegalPosition(GetCenterPosRect(targetRect.position, targetRect.size), token).IsLegal)
                return centerPos;

            foreach (var point in currentPoints)
			{
                if (IsLegalPosition(GetCenterPosRect(point, targetRect.size), token).IsLegal)
                    return point;
            }

            NoonUtility.Log("Choreographer: No legal tabletop position found for " + token.Payload.Id + " (" + centerPos + ")!",1);

            return Vector2.zero;
        }

        // Tokens have their pos in their center, rects in the bottom right
        Rect GetRectWithSpherePosition(RectTransform rectTrans)
		{
            return new Rect(rectTrans.anchoredPosition, rectTrans.rect.size);
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

        void ShowRect(Rect rect,string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            var rectWorldPosition = _tabletop.GetRectTransform().TransformPoint(rect.position);
            var rectScreenPosition= RectTransformUtility.WorldToScreenPoint( Camera.main, rectWorldPosition);

            var guiRect=new Rect(rectScreenPosition,rect.size);

            rectanglesToDisplay[name] = guiRect;
        }

        void HideRect(string name)
        {
            if (rectanglesToDisplay.ContainsKey(name))
                rectanglesToDisplay.Remove(name);
        }

        public void HideAllRects()
        {
            rectanglesToDisplay.Clear();
        }

        LegalPositionCheckResult IsLegalPosition(Rect candidateRect,  Token placingToken)
		{
          
            if (tableRect.Contains(candidateRect.position + candidateRect.size / 2f) == false)
                
                return LegalPositionCheckResult.Illegal();
            
            Rect otherTokenRect;


            foreach (var otherToken in _tabletop.Tokens.Where(t=>t!=placingToken && !CanTokenBeIgnored(t))) {
                  otherTokenRect = GetRectWithSpherePosition(otherToken.TokenRectTransform);

             if (otherTokenRect.Overlaps(candidateRect))
                {
                    return LegalPositionCheckResult.Blocked(otherToken.name,otherTokenRect);
                }
            }

            return LegalPositionCheckResult.Legal();
        }

        bool CanTokenBeIgnored(Token token) {
            
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
