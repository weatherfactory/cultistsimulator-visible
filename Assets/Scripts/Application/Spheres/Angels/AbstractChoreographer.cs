using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using JetBrains.Annotations;
using SecretHistories.Choreographers;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Spheres.Angels
{
 public abstract class AbstractChoreographer: MonoBehaviour
 {
    protected class DebugRect
     {
         public string Desc { get; set; }
         public Rect Rect { get; set; }
         public Color Colour { get; set; }

     }
     public void OnGUI()
     {


         if (showDebugInfo)
             ShowDebugPlacementInfo();
     }

     private Texture2D TextureForColour(Color col)
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

        protected void ShowDebugPlacementInfo()
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
     [SerializeField] private bool showDebugInfo;
        protected List<DebugRect> rectanglesToDisplay = new List<DebugRect>();
        public Sphere Sphere => gameObject.GetComponent<Sphere>();

        public abstract void PlaceTokenAtFreeLocalPosition(Token token, Context context);

        /// <summary>
        /// Place as close to a specific position as we can get
        /// </summary>
        public abstract void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 targetPosition);

        public abstract LegalPositionCheckResult IsLegalPlacement(Rect candidateRect, Token placingToken);

      public abstract  Vector2  GetClosestFreeLocalPosition(Token token, Vector2 startPos);

      public virtual bool CanTokenBeIgnored(Token token)
      {

          if (token.Defunct)
              return true;
          if (token.NoPush)
              return true;

            return false;
      }

      protected float GetGridSnapCoefficient()
      {
          var snapSetting = Watchman.Get<Compendium>().GetEntityById<Setting>(NoonConstants.GRIDSNAPLEVEL);
          

            var snapValue = (float)snapSetting.CurrentValue;

          if (snapValue == 1f)
              return 1f;
          else if (snapValue == 2f)
              return 0.5f;
          else if (snapValue == 3f)
              return 0.25f;
          else if (snapValue == 4f)
              return 0.125f;
            else if (snapValue == 0f)
              return 0.125f; //set at smallest value
          else
          {
              NoonUtility.LogWarning($"Unexpected snapvalue {snapValue}: assuming it means grid set at 0.125f. ");
              return 0.125f;
          }
      }

      protected bool UnacceptableOverlap(Rect rect1, Rect rect2,float overlapModifier)
      {
          rect1.size *= overlapModifier;
          rect2.size *= overlapModifier;
          return rect1.Overlaps(rect2);
      }

      public Vector3 SnapToGrid(Vector2 intendedPos,Token forToken,float gridWidth, float gridHeight)
      {
          //grid: 150x150
          //verb: 140x140 with 5 x space and 5 y space
          //card: 75x115 with 0 x space and 17.5 y space
          //forToken isn't currently in use, but we might treat tokens differently later   

          if (GetGridSnapCoefficient() > 0f)
          {
              float snap_x_interval = gridWidth * GetGridSnapCoefficient();
              float snap_y_interval = gridHeight * GetGridSnapCoefficient();

              float xAdjustment = intendedPos.x % snap_x_interval;
              float yAdjustment = intendedPos.y % snap_y_interval;

              var snappedPos=new Vector2(intendedPos.x-xAdjustment,intendedPos.y-yAdjustment);

              return snappedPos;
          }
          return intendedPos;
      }

      public List<Rect> GetAlternativeCandidateRectsAlongVector(Rect startingRect, Vector2 alongVector, int fromIteration, int toIteration,float gridWidth, float gridHeight)
      {
          List<Rect> candidateRects = new List<Rect>();
          float shiftWidth = gridWidth * GetGridSnapCoefficient();
          float shiftHeight = gridHeight * GetGridSnapCoefficient();

          for (int i=fromIteration;i<=toIteration;i++)
          {
              float shiftX = shiftWidth * alongVector.x * i;
              float shiftY = shiftHeight * alongVector.y * i;

              var candidatePoint = startingRect.position + new Vector2(shiftX, shiftY);
              var candidateRect = new Rect(candidatePoint, startingRect.size);

              candidateRects.Add(candidateRect);

              //   ShowDebugRect(candidateRect, $"{candidatePoint}", Color.white);
          }

          return candidateRects;

      }

      protected Rect GetSphereRect()
      {
          return Sphere.GetRect();
      }
 }
}
