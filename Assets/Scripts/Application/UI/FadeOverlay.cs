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
        [SerializeField] private CanvasGroupFader fader;

        public void Awake()
        {
            var w=new Watchman();
            w.Register(this);
        }

        public void FadeToBlack(float duration)
        {
            fader.durationTurnOn = duration;
                 fader.Show();
        }

    }
}
