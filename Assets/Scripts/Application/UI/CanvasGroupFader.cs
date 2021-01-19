using System;
using System.Collections;
using UnityEngine;

namespace SecretHistories.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupFader : MonoBehaviour {

        public bool blockRaysDuringFade = false;

        public float durationTurnOn;
        public float durationTurnOff;

        private Coroutine _appearingCoroutine;
        private Coroutine _disappearingCoroutine;


        public bool IsVisible()
        {
            return Group.alpha >=1f;
        }

        public bool IsInvisible()
        {
            return Group.alpha <= 0f;
        }


 
        CanvasGroup group;
        CanvasGroup Group {
            get {
                if (group == null)
                    group = GetComponent<CanvasGroup>();

                return group;
            }
        }

        public void Hide()
        {
            if (IsInvisible())
                return;

            if (_appearingCoroutine != null)
                StopCoroutine(_appearingCoroutine);

            if (durationTurnOff <= 0f) {
                SetFinalAlpha(0f);
            }
            else 
                _disappearingCoroutine = StartCoroutine(DoTransparencyChange(0f, durationTurnOff));
        }


        public void Show()
        {
            if (IsVisible())
                return;

            if(_disappearingCoroutine!=null)
                StopCoroutine(_disappearingCoroutine);


            if (durationTurnOn <= 0f) {
                SetFinalAlpha(1f);
            }
            else
                _appearingCoroutine = StartCoroutine(DoTransparencyChange(1f, durationTurnOn));
        }

        IEnumerator DoTransparencyChange(float targetAlpha, float duration) {
            float currentAlpha = Group.alpha;
            float currentTime = 0f;

            SetInteractable(blockRaysDuringFade); 
            duration = duration * Mathf.Abs(targetAlpha - currentAlpha);

            while (currentTime <= duration) {
                Group.alpha = Mathf.Lerp(currentAlpha, targetAlpha, currentTime/duration);
                currentTime += Time.deltaTime;
                yield return null;
            }

            SetFinalAlpha(targetAlpha);
    
        }

        public void SetFinalAlpha(float alpha) {
            if(_appearingCoroutine!=null)
                StopCoroutine(_appearingCoroutine);
       
            if (_disappearingCoroutine != null)
                StopCoroutine(_disappearingCoroutine);

            Group.alpha = alpha;

            if (Mathf.Approximately(alpha, 0f))
                SetInteractable(false);
            else
                SetInteractable(true);
        }

        void SetInteractable(bool state) {
            Group.blocksRaycasts = state;
            Group.interactable = state;
        }

    }
}
