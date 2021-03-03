using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Ghosts
{
    public class NullGhost : AbstractGhost
    {

        public override void ShowAt(RectTransform parentTransform, Vector3 anchoredPosition3D)
        {
//
        }

        public override void HideIn(Token forToken)
        {
            rectTransform.SetParent(forToken.TokenRectTransform); //so it doesn't clutter up the hierarchy
        }


    }
}