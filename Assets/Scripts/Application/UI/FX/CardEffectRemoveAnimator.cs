using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.UI {
    public class CardEffectRemoveAnimator : CardEffectRemove {

        [SerializeField] float fadeDuration = 0.2f;

        public void FadeOutOriginalCard() {
            StartCoroutine(DoFadeOutOriginalCard());
        }

        IEnumerator DoFadeOutOriginalCard() { 
            var time = 0f;

            while (time < fadeDuration) {
                time += Time.deltaTime;
                tokenCanvasGroup.alpha = 1f - time / fadeDuration;
                yield return null;
            }

            tokenCanvasGroup.alpha = 0f;
        }

    }
}
