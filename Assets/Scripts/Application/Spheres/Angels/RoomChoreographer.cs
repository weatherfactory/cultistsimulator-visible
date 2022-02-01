using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Choreographers;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Angels
{
 //place tokens only where a Floor surface is present in the room
    public class RoomChoreographer: AbstractChoreographer
    {
        public override void PlaceTokenAtFreeLocalPosition(Token token, Context context)
        {
            var defaultPosition = Vector3.zero;
            token.TokenRectTransform.anchoredPosition3D = GetFreeLocalPosition(token, defaultPosition);
        }

        public override void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 targetPosition)
        {

            Vector2 closestPosition = GetFreeLocalPosition(token, targetPosition);

            Vector3 finalPositionAtTableLevel = new Vector3(closestPosition.x, closestPosition.y, Sphere.transform.position.z);
            token.TokenRectTransform.localPosition = finalPositionAtTableLevel;
        }

        public override LegalPositionCheckResult IsLegalPlacement(Rect candidateRect, Token placingToken)
        {
            return LegalPositionCheckResult.Legal();
        }

        public override Vector2 GetFreeLocalPosition(Token token, Vector2 startPos)
        {
            var nearestWalkable = AstarPath.active.GetNearest(startPos); // this won't work exactly as I want unless each node is a furniture-placeable position - I think.
            var localPosition = Sphere.transform.InverseTransformPoint(nearestWalkable.position);
            return localPosition;
        }
    }
}
