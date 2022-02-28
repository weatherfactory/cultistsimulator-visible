using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Ghosts;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Ghosts
{
    public class BookGhost : AbstractGhost
    {
        private const float heightToSpine = 0.33f;
        private const float heightToCover = 1f;

        [SerializeField]
        private RectTransform rectTransform;
        public override void Emphasise()
        {
            float height = rectTransform.rect.height;
            rectTransform.sizeDelta = new Vector2(height * heightToCover, height);
        }
        public override void Understate()
        {
            float height = rectTransform.rect.height;
            rectTransform.sizeDelta = new Vector2(height * heightToSpine, height);
        }
    }
}
