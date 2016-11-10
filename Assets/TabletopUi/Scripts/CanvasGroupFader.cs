using System.Collections;
using UnityEngine;

namespace Assets.CS.TabletopUI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupFader : MonoBehaviour {

        public bool destroyOnHide = false;
        public float durationTurnOn = 0.5f;
        public float durationTurnOff = 0.25f;

        CanvasGroup group;
        CanvasGroup Group { 
            get { 
                if (group == null)
                    group = GetComponent<CanvasGroup>();

                return group; 
            } 
        }



        public void Hide() {

            if (durationTurnOn <= 0f) {
                SetAlpha(0f);
            }
            else if (gameObject.activeInHierarchy == false) {
                SetAlpha(0f);
            }
            else if (gameObject.activeSelf && Group.alpha > 0)
            {

                StopAllCoroutines();
                StartCoroutine(DoFade(0f, durationTurnOff));
            }
        }


        public void Show() {
            if (durationTurnOn <= 0f) {
                SetAlpha(1f);
            }
            else if (gameObject.activeSelf == false) {
                gameObject.SetActive(true);
                Group.alpha = 0f;
            }

            if (Group.alpha < 1f) {
                StopAllCoroutines();
                StartCoroutine(DoFade(1f, durationTurnOn));
            }
        }
	
        IEnumerator DoFade(float alpha, float duration) {
            float currentAlpha = Group.alpha;
            float currentTime = 0f;

            SetInteractable(false);
            duration = duration * Mathf.Abs(alpha - currentAlpha);

            while (currentTime <= duration) {
                Group.alpha = Mathf.Lerp(currentAlpha, alpha, currentTime/duration);
                currentTime += Time.deltaTime;
                yield return null;
            }
            SetAlpha(alpha);
        }
	
        public void SetAlpha(float alpha) {
            StopAllCoroutines();
            Group.alpha = alpha;
		
            if (Mathf.Approximately(alpha, 0f)) {
                if (destroyOnHide)
                    Destroy(gameObject);
                else
                    gameObject.SetActive(false);
            }
            else
                SetInteractable(true);
        }

        void SetInteractable(bool state) {
            Group.blocksRaycasts = state;
        }

    }
}
