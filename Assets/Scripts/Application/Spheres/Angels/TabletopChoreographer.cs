﻿using System;
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
     [SerializeField] private bool showGrid;


        private const float GRID_WIDTH = 150f;
     private const float GRID_HEIGHT = 150f;
        


        private List<DebugRect> rectanglesToDisplay=new List<DebugRect>();


        public void OnGUI()
        {

            if (showGrid)
            
                ShowGrid();
            
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

        private void ShowGrid()
        {
            var gridStyle = GUI.skin.box;
            Color gridColor = Color.gray;
            gridColor.a = 0.2f;
            gridStyle.normal.background = TextureForColour(gridColor);

            float GridMaxX = GRID_WIDTH * 10;
            float GridMaxY = GRID_HEIGHT * 10;

            bool showAlternateSquare = false;

            for (int x = 0; x <= 10; x++)
            {
                for (int y = 0; y <= 10; y++)
                {
                    showAlternateSquare = !showAlternateSquare;

                    if (showAlternateSquare)
                    {
                        var gridSquare = new Rect(GRID_WIDTH * x, GRID_HEIGHT * y, GRID_WIDTH, GRID_HEIGHT);
                        GUI.Box(gridSquare, GUIContent.none, gridStyle);
                    }
                }
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


            Vector2 direction = (intendedPos - legalPositionCheckResult.BlockerRect.center).normalized;

       


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
            float shiftWidth = startingRect.width * GetGridSnapCoefficient();
            float shiftHeight = startingRect.height * GetGridSnapCoefficient();

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
            
            if (GetGridSnapCoefficient() > 0f)
            {
                float snap_x_interval = forToken.TokenRectTransform.rect.width * GetGridSnapCoefficient();
                float snap_y_interval = forToken.TokenRectTransform.rect.height * GetGridSnapCoefficient();

                float xAdjustment = intendedPos.x % snap_x_interval;
                float yAdjustment = intendedPos.y % snap_y_interval;

                var snappedPos=new Vector2(intendedPos.x-xAdjustment,intendedPos.y-yAdjustment);

                return snappedPos;
            }
            return intendedPos;
        }

 



    }
}
