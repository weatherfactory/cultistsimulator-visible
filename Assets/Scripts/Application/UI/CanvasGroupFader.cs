using System;
using System.Collections;
using UnityEngine;

namespace SecretHistories.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupFader : MonoBehaviour {

        public bool blockRaysDuringFade = false;
      //  public bool destroyOnHide = false;
      //  public bool keepActiveOnHide = false;  // Only used if destroyOnHide is false
        public float durationTurnOn = 0.5f;
        public float durationTurnOff = 0.25f;

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


        public bool Appearing()
        {

            return _appearingCoroutine != null;
        }


        public bool Disappearing()
        {

            return _disappearingCoroutine != null;
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
            if (IsInvisible() && !Appearing())
                return;
  
            if (durationTurnOff <= 0f) {
                StopAllCoroutines();
                SetFinalAlpha(0f);
            }
            else if (_disappearingCoroutine==null)
            {
                _disappearingCoroutine = StartCoroutine(DoTransparencyChange(0f, durationTurnOff));
            }
        }


        public void Show()
        {
            if (IsVisible() && !Disappearing() )
                return;
            
            if (durationTurnOn <= 0f) {
                StopAllCoroutines();
                SetFinalAlpha(1f);
            }
            else if (_appearingCoroutine == null)
            {
                _appearingCoroutine = StartCoroutine(DoTransparencyChange(1f, durationTurnOn));
            }
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
        StopAllCoroutines();

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
