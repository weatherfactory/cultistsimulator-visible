using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    public class CardEffectRemoveAnimator : CardEffectRemove {

        [SerializeField] float fadeDuration = 0.2f;

        public void FadeOutOriginalCard() {
            StartCoroutine(DoFadeOutOriginalCard());
        }

        IEnumerator DoFadeOutOriginalCard() { 
            var time = 0f;

            while (time < fadeDuration) {
                time += Time.deltaTime;
                targetCard.alpha = 1f - time / fadeDuration;
                yield return null;
            }

            targetCard.alpha = 0f;
        }

    }
}
