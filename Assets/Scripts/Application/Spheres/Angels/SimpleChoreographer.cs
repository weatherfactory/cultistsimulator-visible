using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using SecretHistories.Constants;
using UnityEngine;

namespace SecretHistories.Spheres.Angels
{
    public class SimpleChoreographer:IChoreographer
    {


        public void PlaceTokenAtFreeLocalPosition(Token token, Context context)
        {
            token.TokenRectTransform.anchoredPosition3D = Vector3.zero;
        }

        public void PlaceTokenAssertivelyAtSpecifiedLocalPosition(Token token, Context context, Vector2 pos)
        {
            token.TokenRectTransform.anchoredPosition3D = pos;

        }

        public void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 pos)
        {
            token.TokenRectTransform.anchoredPosition3D = pos;
        }

        public Vector2 GetFreeLocalPosition(Token token, Vector2 centerPos, int startIteration = -1)
        {
            return Vector3.zero;
        }

        public Vector3 SnapToGrid(Vector3 transformLocalPosition)
        {
           
           return Vector3.zero;
        }
    }
}
