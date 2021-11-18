using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Choreographers;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Elements;
using SecretHistories.NullObjects;
using UnityEngine;

namespace SecretHistories.Spheres.Angels
{
    public class SimpleChoreographer:AbstractChoreographer
    {
        public static SimpleChoreographer Create()
        {

                var obj = new GameObject(nameof(SimpleChoreographer));
                var simpleChoreographerComponent = obj.AddComponent<SimpleChoreographer>();

            return simpleChoreographerComponent;
        }

        public override void PlaceTokenAtFreeLocalPosition(Token token, Context context)
        {
            token.TokenRectTransform.anchoredPosition3D = Vector3.zero;
        }

        public override void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 targetPosition)
        {
            token.TokenRectTransform.anchoredPosition3D = targetPosition;
        }

        public override LegalPositionCheckResult IsLegalPlacement(Rect candidateRect, Token placingToken)
        {
            return LegalPositionCheckResult.Legal();
        }

        public override Vector2 GetFreeLocalPosition(Token token, Vector2 centerPos)
        {
            return Vector3.zero;
        }
    }
}
