using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Abstract;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations
{
    public class BasicManifestation: MonoBehaviour
    {
        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();

        [SerializeField] protected bool rotateOnDrag = true;
        protected float perlinRotationPoint = 0f;
        [SerializeField] protected BasicShadowImplementation shadow;


        public virtual void UpdateLocalScale(Vector3 newScale)
        {
            RectTransform.localScale = newScale;
        }

        public virtual void Retire(RetirementVFX retirementVfx, Action callbackOnRetired)
        {


            Destroy(gameObject);
            callbackOnRetired();

        }
 


   
            public virtual void OnBeginDragVisuals(Token token)
        {

            if(shadow!=null)
                shadow.gameObject.SetActive(true);

        }


        public virtual void OnEndDragVisuals(Token token)
        {
            if (rotateOnDrag)
                transform.rotation = token.Sphere.GetRectTransform().rotation;
            if (shadow != null)
                shadow.gameObject.SetActive(false);
        }

        public virtual void DoMove(PointerEventData eventData,RectTransform tokenRectTransform)
        {
            // rotate object slightly based on pointer Delta
            if (rotateOnDrag && eventData.delta.sqrMagnitude > 10f)
            {
                // This needs some tweaking so that it feels more responsive, physical. Card rotates into the direction you swing it?
                perlinRotationPoint += eventData.delta.sqrMagnitude * 0.001f;
                transform.localRotation =
                    Quaternion.Euler(new Vector3(0, 0, -10 + Mathf.PerlinNoise(perlinRotationPoint, 0) * 20));
            }
        }

    }
}
