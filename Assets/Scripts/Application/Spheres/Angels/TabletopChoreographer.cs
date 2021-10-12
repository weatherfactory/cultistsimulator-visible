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
   
        private float gridSnapSize = 0.0f;
        const int maxGridIterations = 5;



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

            var pushingRect = pushingToken.GetRectInCurrentSphere();
	
			Rect pushedRect;

            foreach (var token in _tabletop.Tokens) {
                if (token==pushingToken || CanTokenBeIgnored(token))
                    continue;

                pushedRect = token.GetRectInCurrentSphere();

				if (!UnacceptableOverlap(pushedRect,pushingRect))
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

        public Vector2 GetFreeLocalPosition(Token token, Vector2 intendedPos)
        {
            HideAllDebugRects(); //if we're beginning another attempt to find a free local position, hide all existing debug information

            Vector2 intendedPosClampedToTable = GetPosClampedToTable(intendedPos);
            Vector2 intendedPosOnGrid = SnapToGrid(intendedPosClampedToTable, token);

            var targetRect = token.GetRectAssumingPosition(intendedPosOnGrid);

            var legalPositionCheckResult = IsLegalPlacement(targetRect, token);
            if (legalPositionCheckResult.IsLegal)
            {
                HideAllDebugRects();
                ShowDebugRect(targetRect, $"{token.name} goes here )");
                return intendedPosOnGrid;
            }
            else
            {
                ShowDebugRect(targetRect, $"{token.name} would overlap)");
                ShowDebugRect(legalPositionCheckResult.BlockerRect,
                    $"{legalPositionCheckResult.BlockerName} is overlapping default position for {token.name}");
            }


            int currentIteration = 1;

            while (currentIteration < 10)
            {


                var testRects = GetTestRects(targetRect, currentIteration,token);

                // Iterate over a single round of test positions. If one is legal, then return it.
                foreach (var testRect in testRects)
                {
                    if (IsLegalPlacement(testRect, token).IsLegal)
                        return testRect.position +
                               targetRect.size / 2f; //this assumes that the position is in the centre of the rect
                }

                currentIteration++;
            }

            NoonUtility.Log(
                $"Choreographer: No legal tabletop position found for {token.name})! Just putting it at zero", 1);


            return Vector2.zero;

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

       private bool UnacceptableOverlap(Rect rect1, Rect rect2)
       {
            //we require grid snap. 'No grid snap' is no longer an option.
            //Grid snap 1 means cards cannot overlap at all.
            //Grid snap 0.5 means cards can overlap up to 50%.
            //Grid snap 0.25 means cards can overlap up to 75%.
            return rect1.Overlaps(rect2);
       }

        public void HideAllDebugRects()
        {
            rectanglesToDisplay.Clear();
        }

        LegalPositionCheckResult IsLegalPlacement(Rect candidateRect,  Token placingToken)
		{
          //Is the candidaterect inside the larger tabletop rect? if not, throw it out now.
            if (!GetTableRect().Overlaps(candidateRect))
                return LegalPositionCheckResult.Illegal();
            
            Rect otherTokenOverlapRect;


            foreach (var otherToken in _tabletop.Tokens.Where(t=>t!=placingToken && !CanTokenBeIgnored(t)))
            {
                otherTokenOverlapRect = otherToken.GetRectInCurrentSphere();

             if (UnacceptableOverlap(otherTokenOverlapRect,candidateRect))
                
                   return LegalPositionCheckResult.Blocked(otherToken.name,otherTokenOverlapRect);
                
            }

            //foreach(var itinerary  in Watchman.Get<Xamanek>().CurrentItinerariesForPath(_tabletop.GetAbsolutePath()))
            //{
            //    if(itinerary.GetGhost().PromiseBlocksCandidateRect(_tabletop,candidateRect))
            //        return LegalPositionCheckResult.Blocked($"Reserved destination for {itinerary.GetDescription()}", itinerary.GetGhost().GetRect());
            //}

            return LegalPositionCheckResult.Legal();
        }

        bool CanTokenBeIgnored(Token token) {
            
            if (token.Defunct)
                return true;
			if (token.NoPush)
				return true;

            return false;
        }

        private List<Rect> GetTestRects(Rect startingRect, int iteration,Token rectsForToken)
        {
            
            List <Rect> rects= new List<Rect>();


            Vector2 aboveRectPosition=new Vector2(startingRect.x,startingRect.y+(startingRect.height));
           Rect aboveRect=new Rect(aboveRectPosition,startingRect.size);

            ShowDebugRect(aboveRect,$"AboveRect for {rectsForToken.name}");

            rects.Add(aboveRect);

           Vector2 belowRectPosition = new Vector2(startingRect.x, startingRect.y - (startingRect.height));
           Rect belowRect = new Rect(belowRectPosition, startingRect.size);

           rects.Add(belowRect);
           ShowDebugRect(belowRect, $"BelowRect for {rectsForToken.name}");

            return rects;

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



        public Vector3 SnapToGrid(Vector2 intendedPos,Token forToken)
        {
            
            if (GetGridSnapSize() > 0f)
            {
                float snap_x_interval = forToken.TokenRectTransform.rect.width * GetGridSnapSize();
                float snap_y_interval = forToken.TokenRectTransform.rect.height * GetGridSnapSize();

                float xAdjustment = intendedPos.x % snap_x_interval;
                float yAdjustment = intendedPos.y % snap_y_interval;

                var snappedPos=new Vector2(intendedPos.x-xAdjustment,intendedPos.y-yAdjustment);

                return snappedPos;
            }
            return intendedPos;
        }

        public void WhenSettingUpdated(object newValue)
        {
            SetGridSnapSize(newValue is float ? (float)newValue : 0);
        }




    }
}
