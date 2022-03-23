using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
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

        [SerializeField] private GameObject frontCover;
        [SerializeField]
        private Image frontCoverImage;
        [SerializeField]
        private TextMeshProUGUI frontCoverTitle;



        [SerializeField]
        private GameObject spine;
        [SerializeField]
        private Image spineImage;
        [SerializeField]
        private TextMeshProUGUI spineTitle;



        public void Emphasise()
        {
            frontCover.SetActive(true);
            spine.SetActive(false);
        }

        public void Understate()
        {
            frontCover.SetActive(false);
            spine.SetActive(true);
        }

        public override void UpdateVisuals(IManifestable manifestable)
        {
            Sprite f = ResourcesManager.GetSpriteForFrontCover(manifestable.Icon);
            frontCoverImage.sprite = f;
            Sprite s = ResourcesManager.GetSpriteForSpine(manifestable.Icon);
            spineImage.sprite = s;

            spineTitle.text = manifestable.Label;
            frontCoverTitle.text = manifestable.Label;

        }
    }
}
