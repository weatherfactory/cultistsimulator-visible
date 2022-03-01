using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Enums;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Manifestations
{
    public class BasicManifestation: MonoBehaviour
    {
        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();

        public virtual void UpdateLocalScale(Vector3 newScale)
        {
            RectTransform.localScale = newScale;
        }

        public virtual void Retire(RetirementVFX retirementVfx, Action callbackOnRetired)
        {


            Destroy(gameObject);
            callbackOnRetired();

        }

    }
}
