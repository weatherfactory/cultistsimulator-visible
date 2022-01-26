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


     public Sphere Sphere => gameObject.GetComponent<Sphere>();

        public abstract void PlaceTokenAtFreeLocalPosition(Token token, Context context);

        /// <summary>
        /// Place as close to a specific position as we can get
        /// </summary>
        public abstract void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 targetPosition);

        public abstract LegalPositionCheckResult IsLegalPlacement(Rect candidateRect, Token placingToken);

      public abstract  Vector2  GetFreeLocalPosition(Token token, Vector2 startPos);

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
          var snapValue = (float) Watchman.Get<Compendium>().GetEntityById<Setting>(NoonConstants.GRIDSNAPSIZE).CurrentValue;

          if (snapValue == 1f)
              return 1f;
          else if (snapValue == 2f)
              return 0.5f;
          else if (snapValue == 3f)
              return 0.25f;
          else if (snapValue == 0f)
              return 1f; //assuming unset
          else
          {
              NoonUtility.LogWarning($"Unexpected snapvalue {snapValue}: assuming it means grid set at 1. ");
              return 1f;
          }
      }

      protected bool UnacceptableOverlap(Rect rect1, Rect rect2,float overlapModifier)
      {
          rect2.size *= overlapModifier;
          return rect1.Overlaps(rect2);
      }
    }
}
