using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        class DebugRect
        {
            public string Desc { get; set; }
            public Rect Rect { get; set; }

            }
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

        private Rect GetTableRect()
        {
            return _tabletop.GetRect();
        }
     [SerializeField] private bool showDebugInfo;
        const float checkPointPerArcLength = 100f;

        const float pointGridSize = 100f;
        private float gridSnapSize = 0.0f;
        const int maxGridIterations = 5;

        const float radiusBase = 50f;
        const float radiusIncrement = 50f;
        const float radiusMaxSize = 250f;

        private List<DebugRect> rectanglesToDisplay=new List<DebugRect>();

        public void Awake() {

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
            if (!showDebugInfo)
                return;
            foreach (var r in rectanglesToDisplay)
            {
                var style = GUI.skin.box;
                style.wordWrap = true;
                GUI.Box(r.Rect, r.Desc, style);
            }
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
/// Place as close to a specific position as we can get
/// </summary>
        public void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 pos)
{
    Vector2 freeLocalPosition= GetFreeLocalPosition(token, pos);
    //token.TokenRectTransform.anchoredPosition = freeLocalPosition;
            Vector3 finalPositionAtTableLevel=new Vector3(freeLocalPosition.x,freeLocalPosition.y,_tabletop.transform.position.z);
            token.TokenRectTransform.localPosition = finalPositionAtTableLevel;



        }

public void MoveAllTokensOverlappingWith(Token pushingToken)
		{
			if (pushingToken.NoPush)
			{
				return;
			}

            var pushingRect = pushingToken.TokenRectTransform.rect;
	
			Rect pushedRect;

            foreach (var token in _tabletop.Tokens) {
                if (token==pushingToken || CanTokenBeIgnored(token))
                    continue;

                pushedRect = token.TokenRectTransform.rect;

				if (!pushedRect.Overlaps(pushingRect))
                    continue;

                var freePositionForPushedToken = GetFreeLocalPosition(token, token.TokenRectTransform.anchoredPosition);

                TokenTravelItinerary itinerary=new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D, freePositionForPushedToken)
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

        public Vector2 GetFreeLocalPosition(Token token, Vector2 startPos)
		{
                HideAllDebugRects(); //if we're beginning another attempt to find a free local position, hide all existing debug information
            
            Vector2 centerPosition = GetPosClampedToTable(startPos);
           Vector2 snappedToGridPosition = SnapToGrid(centerPosition);
            var targetRect = GetLocalRectFromCenterPosition(snappedToGridPosition, token.TokenRectTransform.rect.size);

            var legalPositionCheckResult = IsLegalPosition(targetRect, token);
            if (legalPositionCheckResult.IsLegal)
            {
                HideAllDebugRects();
                ShowDebugRect(targetRect, $"{token.name} goes here )");
                return snappedToGridPosition;
            }
            else
            {
                ShowDebugRect(targetRect, $"{token.name} would overlap)");
                ShowDebugRect(legalPositionCheckResult.BlockerRect,
                    $"{legalPositionCheckResult.BlockerName} is overlapping default position for {token.name}");
            }


            int currentIteration = 1;
            var currentPosition = GetTestPositions(targetRect.position + targetRect.size / 2f, currentIteration, maxGridIterations);

            // Iterate over a single round of test positions. If one is legal, then return it.
            foreach (var position in currentPosition)
			{
                if (IsLegalPosition(GetLocalRectFromCenterPosition(position, targetRect.size), token).IsLegal)
                    return position;
            }
            
            return ReturnPositionFromExpandedSearch(token, snappedToGridPosition, targetRect, currentIteration+1);
        }

        private Vector2 ReturnPositionFromExpandedSearch(Token token, Vector2 snappedToGridPosition, Rect targetRect,
            int currentIteration)
        {
            Vector2[] currentPosition;
            // we've exhausted things, so we do again
            // but we  shift the target pos a bit
            Vector2 newStartPosition = snappedToGridPosition + targetRect.size;
  
            //Debug.Log("Did not find a legal pos for " + token.Id + ", allowing for overlap!");

            // request a new set of points, since the center pos has shifted
            currentPosition = GetTestPositions(targetRect.position + targetRect.size / 2f, currentIteration, maxGridIterations);

            var centerPosRect = GetLocalRectFromCenterPosition(targetRect.position, targetRect.size);
            if (IsLegalPosition(centerPosRect, token).IsLegal)
                return newStartPosition;

            foreach (var point in currentPosition)
            {
                if (IsLegalPosition(GetLocalRectFromCenterPosition(point, targetRect.size), token).IsLegal)
                    return point;
            }

            NoonUtility.Log(
                "Choreographer: No legal tabletop position found for " + token.Payload.Id + " (" + newStartPosition + ")!", 1);

            return Vector2.zero;
        }


        Rect GetLocalRectFromCenterPosition(Vector2 centerPos, Vector2 size)
		{
            return new Rect(centerPos - size / 2f, size);
        }

        Vector2 GetPosClampedToTable(Vector2 pos)
		{
            const float padding = .2f;

            var tableMinX = GetTableRect().x + padding;
            var tableMaxX = GetTableRect().x + GetTableRect().width - padding;
            var tableMinY = GetTableRect().y + padding;
            var tableMaxY = GetTableRect().y + GetTableRect().height - padding;
            pos.x = Mathf.Clamp(pos.x, tableMinX,tableMaxX );
            pos.y = Mathf.Clamp(pos.y, tableMinY,tableMaxY);
            return pos;
        }

       private void ShowDebugRect(Rect rect,string desc)
        {
            if (string.IsNullOrEmpty(desc))
                return;

    
            var rectWorldPosition = _tabletop.GetRectTransform().TransformPoint(rect.position);
            var rectScreenPosition= RectTransformUtility.WorldToScreenPoint( Camera.main, rectWorldPosition);

            var guiRect=new Rect(rectScreenPosition,rect.size);
            guiRect.position = new Vector3(guiRect.position.x, Screen.height - guiRect.position.y,-50);

            rectanglesToDisplay.Add(new DebugRect{Desc= desc, Rect=guiRect});
        }


        public void HideAllDebugRects()
        {
            rectanglesToDisplay.Clear();
        }

        LegalPositionCheckResult IsLegalPosition(Rect candidateRect,  Token placingToken)
		{
          //Is the candidaterect inside the larger tabletop rect? if not, throw it out now.
            if (GetTableRect().Contains(candidateRect.position + candidateRect.size / 2f) == false)
                
                return LegalPositionCheckResult.Illegal();
            
            Rect otherTokenRect;


            foreach (var otherToken in _tabletop.Tokens.Where(t=>t!=placingToken && !CanTokenBeIgnored(t)))
            {
                otherTokenRect = otherToken.TokenRectTransform.rect;

             if (otherTokenRect.Overlaps(candidateRect))
                
                   return LegalPositionCheckResult.Blocked(otherToken.name,otherTokenRect);
                
            }

            foreach(var itinerary  in Watchman.Get<Xamanek>().CurrentItinerariesForPath(_tabletop.GetAbsolutePath()))
            {
                if(itinerary.GetGhost().PromiseBlocksCandidateRect(_tabletop,candidateRect))
                    return LegalPositionCheckResult.Blocked($"Reserved destination for {itinerary.GetDescription()}", itinerary.GetGhost().GetRect());
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

        Vector2[] GetTestPositions(Vector3 pos, int startIteration, int maxIteration)
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
