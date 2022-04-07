using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
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


        [SerializeField]
        private Image spineImage;


        public override void UpdateVisuals(IManifestable manifestable)
        {

            UpdateVisuals(manifestable,NullSphere.Create());

        }

        public override void UpdateVisuals(IManifestable manifestable,Sphere sphere)
        {

            Sprite s = ResourcesManager.GetSpriteForSpine(manifestable.Icon);
            spineImage.sprite = s;

  
            if (sphere.SphereCategory == SphereCategory.World)
            {
                transform.eulerAngles = new Vector3(0, 0, 90);
                ReverseWidthAndHeight();
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                ReverseWidthAndHeight();
            }
        }

        private void ReverseWidthAndHeight()
        {
            var rt = gameObject.GetComponent<RectTransform>();
            var currentSizeDelta = rt.sizeDelta;
            Vector2 newSizeDelta = new Vector2(currentSizeDelta.y, currentSizeDelta.x);
            rt.sizeDelta = newSizeDelta;
        }
    }
}
