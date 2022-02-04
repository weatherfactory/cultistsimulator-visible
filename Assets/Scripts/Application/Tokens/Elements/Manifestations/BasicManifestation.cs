using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens.Elements.Manifestations
{
    public class BasicManifestation: MonoBehaviour
    {
        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();

        public virtual void UpdateLocalScale(Vector3 newScale)
        {
            RectTransform.localScale = newScale;
        }



    }
}
