using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    public class CardBurnEffect : MonoBehaviour {

        [SerializeField] Graphic cardBurnOverlay;
        [SerializeField] ParticleSystem particles;
        [SerializeField] float durationPhase1 = 1f;
        [SerializeField] float phasePause = 0.1f;
        [SerializeField] float durationPhase2 = 0.4f;
        [SerializeField] float pauseEnd = 0.4f;

        Color startColor = new Color(0f, 1f, 1f, 1f);
        Color phase1Color = new Color(0.45f, 1f, 1f, 1f);
        Color phase2Color = new Color(1f, 0f, 1f, 1f);

        private CanvasGroup originalCard;

        public void StartAnim(ElementStackToken card) {
            originalCard = card.GetComponent<CanvasGroup>();

            cardBurnOverlay.transform.SetParent(originalCard.transform);
            cardBurnOverlay.transform.localScale = Vector3.one;
            cardBurnOverlay.transform.localPosition = Vector3.zero;
            cardBurnOverlay.transform.localRotation = Quaternion.identity;

            // Prevent interaction from the player
            originalCard.interactable = false;
            originalCard.blocksRaycasts = false;

            cardBurnOverlay.gameObject.SetActive(true);

            StopAllCoroutines();
            StartCoroutine(DoBurnAnim());
        }

        IEnumerator DoBurnAnim() {
            float time = 0f;
            cardBurnOverlay.color = startColor;
            particles.Play();

            while (time < durationPhase1) {
                time += Time.deltaTime;
                cardBurnOverlay.color = Color.Lerp(startColor, phase1Color, time / durationPhase1);
                yield return null;
            }

            cardBurnOverlay.color = phase1Color;

            if (phasePause > 0f)
                yield return new WaitForSeconds(phasePause);

            time = 0f;

            while (time < durationPhase2) {
                time += Time.deltaTime;
                cardBurnOverlay.color = Color.Lerp(phase1Color, phase2Color, time / durationPhase2);
                originalCard.alpha = 1f - time / durationPhase2 * 1.5f;
                yield return null;
            }

            cardBurnOverlay.color = phase2Color;
            originalCard.alpha = 0f;

            if (pauseEnd > 0f)
                yield return new WaitForSeconds(pauseEnd);

            Destroy(originalCard.gameObject);
        }

    }
}
