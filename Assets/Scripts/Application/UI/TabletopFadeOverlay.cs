using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.UI
{
    /// <summary>
    /// use this for a non-standard in-scene fade. The standard fade-to-black between scenes is handled by StageHand
    /// </summary>
    public class TabletopFadeOverlay: MonoBehaviour
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
