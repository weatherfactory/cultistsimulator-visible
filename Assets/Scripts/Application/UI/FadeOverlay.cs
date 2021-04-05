using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.UI
{
    public class FadeOverlay: MonoBehaviour
    {
        [SerializeField] private Image image;

        public void Awake()
        {
            var w=new Watchman();
            w.Register(this);
        }

        public void FadeToBlack(float duration)
        {
            gameObject.SetActive(true);
            image.canvasRenderer.SetAlpha(0f);
                 image.CrossFadeAlpha(1f, duration, true);
        }

    }
}
