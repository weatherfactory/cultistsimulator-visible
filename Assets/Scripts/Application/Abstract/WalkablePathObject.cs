using System.Collections;
using System.Runtime.Remoting.Messaging;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Abstract
{
    public abstract class WalkablePathObject: MonoBehaviour
    {
        public abstract bool TokenAllowedHere(Token token);

        public float anchorX
        {
            get
            {
                var rt = gameObject.GetComponent<RectTransform>();
                return rt.anchoredPosition.x;

            }
        }
        public float anchorY
        {
            get
            {
                var rt = gameObject.GetComponent<RectTransform>();
                return rt.anchoredPosition.y;

            }
        }
    }
}