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




        [SerializeField]
        private GameObject spine;
        [SerializeField]
        private Image spineImage;





        public override void UpdateVisuals(IManifestable manifestable)
        {

            Sprite s = ResourcesManager.GetSpriteForSpine(manifestable.Icon);
            spineImage.sprite = s;

        }
    }
}
