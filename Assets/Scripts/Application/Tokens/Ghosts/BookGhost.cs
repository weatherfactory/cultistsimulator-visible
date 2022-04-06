using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Enums;
using SecretHistories.Ghosts;
using SecretHistories.Spheres;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.Ghosts
{
    public class BookGhost : AbstractGhost
    {
        private const float heightToSpine = 0.33f;
        private const float heightToCover = 1f;
        [SerializeField]
        private bool _toppled=false;

        [SerializeField] private GameObject frontCover;
        [SerializeField]
        private Image frontCoverImage;




        [SerializeField]
        private GameObject spine;
        [SerializeField]
        private Image spineImage;

        [SerializeField]
        private GameObject flat;
        [SerializeField]
        private Image flatImage;

        //public override void ShowAt(Sphere projectInSphere, Vector3 showAtAnchoredPosition3D, RectTransform tokenRectTransform)
        //{

        //    if (projectInSphere == _projectedInSphere && rectTransform.anchoredPosition3D != showAtAnchoredPosition3D) //do a smooth transition if moving in the same projected sphere and position not already identical
        //        AnimateGhostMovement(rectTransform.anchoredPosition3D, showAtAnchoredPosition3D);
        //    else
        //    {
        //        rectTransform.SetParent(projectInSphere.GetRectTransform());
        //        rectTransform.localScale = tokenRectTransform.localScale;
        //        //if (projectInSphere.SphereCategory == SphereCategory.World)
        //        //    Topple();
        //        //else
        //        //    StandUp();


        //        rectTransform.anchoredPosition3D = showAtAnchoredPosition3D;
        //        ShowCanvasGroupFader();
        //        _projectedInSphere = projectInSphere;
        //    }
        //}

        private void StandUp()
        {
            _toppled=false;
            rectTransform.eulerAngles = new Vector3(0, 0, 0);
        }

        private void Topple()
        {
            _toppled = true;
            rectTransform.eulerAngles = new Vector3(0, 0, 90);
        }

        public override bool TryFulfilPromise(Token token, Context context)
        {
            if (_toppled)
                token.ManifestationRectTransform.eulerAngles = new Vector3(0, 0, 90);
            else
                token.ManifestationRectTransform.eulerAngles = new Vector3(0, 0, 0);

            return base.TryFulfilPromise(token, context);
        }


        public override void Emphasise()
        {
            frontCover.SetActive(true);
            spine.SetActive(false);
        //    UpdateGhostRect(frontCover);
        }

        public override void Understate()
        {
            frontCover.SetActive(false);
            spine.SetActive(true);
           // UpdateGhostRect(spine);
        }

        private void UpdateGhostRect(GameObject toMatchObject)
        {
            RectTransform newSize = toMatchObject.GetComponent<RectTransform>();
            if (newSize == null)
            {
                NoonUtility.LogWarning($"Trying to update recttransform for bookmanifestation from {toMatchObject.name} but it doesn't have a recttransform");
                return;
            }

            var rt = gameObject.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(newSize.sizeDelta.x,
                newSize.sizeDelta.y);
        }

        public override void UpdateVisuals(IManifestable manifestable)
        {
            Sprite f = ResourcesManager.GetSpriteForFrontCover(manifestable.Icon);
            frontCoverImage.sprite = f;
            Sprite s = ResourcesManager.GetSpriteForSpine(manifestable.Icon);
            spineImage.sprite = s;

 
            Understate(); //for now, stay understated
        }
    }
}
