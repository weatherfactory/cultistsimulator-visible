#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.UI {
    public class CardEffectRemoveColorAnim : CardEffectRemove {

        [SerializeField] Graphic cardBurnOverlay;

        [SerializeField] float durationPhase1 = 1f;
        [SerializeField] float phasePause = 0.1f;
        [SerializeField] float durationPhase2 = 0.4f;
        [SerializeField] float pauseEnd = 0.4f;

        [SerializeField] Color startColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] Color phase1Color = new Color(0.45f, 1f, 1f, 1f);
        [SerializeField] Color phase2Color = new Color(1f, 0f, 1f, 1f);

        public override void StartAnim(Transform token) {
            // Set target card, prevent interaction, position effect, Play Sound
            base.StartAnim(token);

            StopAllCoroutines();
            StartCoroutine(DoBurnAnim()); //This will fail if the card's parent is subsequently disabled. So we use OnDisable, below, to finish it quickly if the card is then disabled.
            //There may be redundant code in other places from previous attempts to fix this.
        }

        IEnumerator DoBurnAnim() {
            float time = 0f;
            cardBurnOverlay.color = startColor;

            while (time < durationPhase1) {
                time += Time.deltaTime;
                cardBurnOverlay.color = Color.Lerp(startColor, phase1Color, time / durationPhase1);
                yield return null;
            }

            cardBurnOverlay.color = phase1Color;
            time = 0f;

            while (time < phasePause) {
                time += Time.deltaTime;
                tokenCanvasGroup.alpha = 1f - time / phasePause * 1.5f;
                yield return null;
            }

            tokenCanvasGroup.alpha = 0f;
            time = 0f;

            while (time < durationPhase2) {
                time += Time.deltaTime;
                cardBurnOverlay.color = Color.Lerp(phase1Color, phase2Color, time / durationPhase2);
                yield return null;
            }

            cardBurnOverlay.color = phase2Color;
            tokenCanvasGroup.alpha = 0f;

            if (pauseEnd > 0f)
                yield return new WaitForSeconds(pauseEnd);

            OnAnimDone();
        }
    }
}
