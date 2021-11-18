using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using JetBrains.Annotations;
using SecretHistories.Choreographers;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Spheres.Angels
{
    public abstract class AbstractChoreographer: MonoBehaviour
    {
        [SerializeField]
        protected Sphere _sphere;


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

      protected bool UnacceptableOverlap(Rect rect1, Rect rect2) //probably needs rework ONCE I'VE REIMPLEMENTED GRIDSNAP
      {
          //we require grid snap. 'No grid snap' is no longer an option.
          //Grid snap 1 means cards cannot overlap at all.
          //Grid snap 0.5 means cards can overlap up to 50%.
          //Grid snap 0.25 means cards can overlap up to 75%.
          return rect1.Overlaps(rect2);
      }
    }
}
