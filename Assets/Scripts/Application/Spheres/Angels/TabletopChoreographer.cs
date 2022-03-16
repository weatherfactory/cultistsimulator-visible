using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SecretHistories.Choreographers;
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

    public class TabletopChoreographer: AbstractChoreographer {

        class DebugRect
        {
            public string Desc { get; set; }
            public Rect Rect { get; set; }
            public Color Colour { get; set; }

        }


        private Rect GetTableRect()
        {
            return Sphere.GetRect();
        }
     [SerializeField] private bool showDebugInfo;

     [SerializeField]
        private  float GRID_WIDTH;
        [SerializeField]
        private float GRID_HEIGHT;
     //grid: 150x150
     //verb: 140x140 with 5 x space and 5 y space
     //card: 75x115 with 0 x space and 17.5 y space
     
     //remember: there are also currently-aligned magic number values in the initial card setup that need to be manually changed until I refactor the gridsnap out into settings


        private List<DebugRect> rectanglesToDisplay=new List<DebugRect>();


        public void OnGUI()
        {

            
            if (showDebugInfo)
             ShowDebugPlacementInfo();
        }

        private void ShowDebugPlacementInfo()
        {
            foreach (var r in rectanglesToDisplay)
            {
                var style = GUI.skin.box;
                Color transparentColor = r.Colour;

                transparentColor.a = 0.3f;
                style.normal.background = TextureForColour(transparentColor);
                style.wordWrap = true;

                GUI.Box(r.Rect, r.Desc, style);
            }
        }



        private Texture2D TextureForColour( Color col)
        {
            int defaultWidth = 2;
            int defaultHeight = 2;
            Color[] pix = new Color[defaultWidth * defaultHeight];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(defaultWidth, defaultHeight);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public void GroupAllStacks()
        {
            var stackTokens = Sphere.GetElementTokens();
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

        public override void PlaceTokenAtFreeLocalPosition(Token token, Context context)
     {
         token.TokenRectTransform.anchoredPosition = GetClosestFreeLocalPosition(token, Vector2.zero);
     }



        /// <summary>
        /// Place as close to a specific position as we can get
        /// </summary>
        public override void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 pos)
{
        Vector2 freeLocalPosition= GetClosestFreeLocalPosition(token, pos);
    
            Vector3 finalPositionAtTableLevel=new Vector3(freeLocalPosition.x,freeLocalPosition.y, Sphere.transform.position.z);
            token.TokenRectTransform.localPosition = finalPositionAtTableLevel;


        }



        public override Vector2 GetClosestFreeLocalPosition(Token token, Vector2 intendedPos)
        {
            HideAllDebugRects(); //if we're beginning another attempt to find a free local position, hide all existing debug information

            if (token.CurrentState.Docked()) //Sometimes the token has been placed assertively or is already present, but has just been accepted into the tabletop sphere - eg if calved. If so, don't worry about overlaps.
                return intendedPos;

            Vector2 intendedPosClampedToTable = GetPosClampedToTable(intendedPos);
            
            Vector2 intendedPosOnGrid = SnapToGrid(intendedPosClampedToTable, token);

            var targetRect = token.GetRectFromPosition(intendedPosOnGrid);

            var legalPositionCheckResult = IsLegalPlacement(targetRect, token);
            if (legalPositionCheckResult.IsLegal)
            {
                return intendedPosOnGrid;
            }
    

            Vector2 direction = (intendedPosOnGrid - legalPositionCheckResult.BlockerRect.center).normalized; //intendedPosOnGrid *not* intendedPos. We're looking for candidate locations starting at the
            //grid position we tried, because that's where the ghost will show up, not the original cursor position, which we've already corrected from and don't want to double-correct from.

       


                var testRects = GetAlternativeCandidateRectsAlongVector(targetRect, direction,1,100);

                // Iterate over a single round of test positions. If one is legal, then return it.
                foreach (var testRect in testRects)
                {
                    if (IsLegalPlacement(testRect, token).IsLegal)
                        return testRect.center;
                        //return testRect.position +
                        //       targetRect.size / 2f; //this assumes that the position is in the centre of the rect
                }

                

            NoonUtility.Log(
                $"Choreographer: No legal tabletop position found for {token.name})! Just putting it at zero", 1);


            return Vector2.zero;

        }

        public List<Rect> GetAlternativeCandidateRectsAlongVector(Rect startingRect, Vector2 alongVector, int fromIteration, int toIteration)
        {
            List<Rect> candidateRects = new List<Rect>();
            float shiftWidth = GRID_WIDTH * GetGridSnapCoefficient();
            float shiftHeight = GRID_HEIGHT * GetGridSnapCoefficient();

            for (int i=fromIteration;i<=toIteration;i++)
            {
                float shiftX = shiftWidth * alongVector.x * i;
                float shiftY = shiftHeight * alongVector.y * i;

                var candidatePoint = startingRect.center + new Vector2(shiftX, shiftY);
                var candidateRect = new Rect(candidatePoint, startingRect.size);

                candidateRects.Add(candidateRect);

                ShowDebugRect(candidateRect, $"{candidatePoint}", Color.white);
            }

            return candidateRects;

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
             ShowDebugRect(rect,desc, Color.blue);
        }

       private void ShowDebugRect(Rect rect, string desc,Color Colour)
       {
           if (string.IsNullOrEmpty(desc))
               return;

           var rectWorldPosition = Sphere.GetRectTransform().TransformPoint(rect.position);
            
           var rectScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, rectWorldPosition);

           var guiRect = new Rect(rectScreenPosition, rect.size);

           //yes I do need to add the height back in. Or do something else transformationally relevant above
           guiRect.position = new Vector3(guiRect.position.x, Screen.height - (guiRect.position.y + rect.height), -50);
           

           rectanglesToDisplay.Add(new DebugRect { Desc = desc, Rect = guiRect,Colour = Colour});
       }



        public void HideAllDebugRects()
        {
            rectanglesToDisplay.Clear();
        }

       public override LegalPositionCheckResult IsLegalPlacement(Rect candidateRect,  Token placingToken)
		{
          //Is the candidaterect inside the larger tabletop rect? if not, throw it out now.
            if (!GetTableRect().Overlaps(candidateRect))
                return LegalPositionCheckResult.Illegal();
            
            

            //If the player is deciding where to place the token, they have NEAR ULTIMATE POWER
       //     if(placingToken.CurrentlyBeingDragged())
         //       return LegalPositionCheckResult.Legal();

            //If some other agency is trying to do it, they're more constricted. Look for a space.

            Rect otherTokenOverlapRect;

            foreach (var otherToken in Sphere.Tokens.Where(t=>t!=placingToken && !CanTokenBeIgnored(t)))
            {
                otherTokenOverlapRect = otherToken.GetRectInCurrentSphere();

                var overlapModifier = 1f;

                if (otherToken.Payload.IsValidElementStack() && placingToken.IsValidElementStack())
                    overlapModifier = GetGridSnapCoefficient();


                if (UnacceptableOverlap(otherTokenOverlapRect,candidateRect, overlapModifier))

                    return LegalPositionCheckResult.Blocked(otherToken.name,otherTokenOverlapRect);
                
            }

            return LegalPositionCheckResult.Legal();
        }


       


        public Vector3 SnapToGrid(Vector2 intendedPos,Token forToken)
        {
            //grid: 150x150
            //verb: 140x140 with 5 x space and 5 y space
            //card: 75x115 with 0 x space and 17.5 y space
         //forToken isn't currently in use, but we might treat tokens differently later   

            if (GetGridSnapCoefficient() > 0f)
            {
                float snap_x_interval = GRID_WIDTH * GetGridSnapCoefficient();
                float snap_y_interval = GRID_HEIGHT * GetGridSnapCoefficient();

                float xAdjustment = intendedPos.x % snap_x_interval;
                float yAdjustment = intendedPos.y % snap_y_interval;

                var snappedPos=new Vector2(intendedPos.x-xAdjustment,intendedPos.y-yAdjustment);

                return snappedPos;
            }
            return intendedPos;
        }

 



    }
}
