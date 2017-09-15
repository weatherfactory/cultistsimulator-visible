using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    public class CardEffectAnimator : CardEffect {

        [SerializeField] string sfx = "CardBurn";
        [SerializeField] float fadeDuration = 0.2f;

        public override void StartAnim(ElementStackToken card) {
            // Set target card, prevent interaction
            base.StartAnim(card);

            transform.SetParent(targetCard.transform);
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            gameObject.SetActive(true);

            if (sfx != null && sfx != "")
                SoundManager.PlaySfx(sfx);
        }

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
