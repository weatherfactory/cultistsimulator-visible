using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine;

namespace Assets.Scripts.Spheres.Angels
{
    public class SimpleChoreographer:IChoreographer
    {
        private readonly Sphere _sphere;

        public void PlaceTokenAtFreePosition(Token token, Context context)
        {
            token.TokenRectTransform.anchoredPosition3D = Vector3.zero;
        }

        public void PlaceTokenAssertivelyAtSpecifiedPosition(Token token, Context context, Vector2 pos)
        {
            token.TokenRectTransform.anchoredPosition3D = pos;

        }

        public void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 pos)
        {
            token.TokenRectTransform.anchoredPosition3D = pos;
        }

        public Vector2 GetFreePosWithDebug(Token token, Vector2 centerPos, int startIteration = -1)
        {
            return Vector3.zero;
        }


    }
}
