using System.Collections;
using UnityEngine;

namespace Assets.CS.TabletopUI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupFader : MonoBehaviour {

        public bool blockRaysDuringFade = false;
      //  public bool destroyOnHide = false;
      //  public bool keepActiveOnHide = false;  // Only used if destroyOnHide is false
        public float durationTurnOn = 0.5f;
        public float durationTurnOff = 0.25f;
        private bool m_isFading;

        private Coroutine transparencyChange;

        public bool IsVisible()
        {
            return Group.alpha > 0f;
        }

        public bool IsChanging() {
            return transparencyChange!=null;
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
            SetAlpha(0f);
            
            if (durationTurnOn <= 0f) {
                SetAlpha(0f);
            }
            else if ( !IsChanging())
            {
                transparencyChange = StartCoroutine(DoTransparencyChange(0f, durationTurnOff));
            }
        }


        public void Show()
        {
            SetAlpha(1f);
            return;
            if (durationTurnOn <= 0f) {
                SetAlpha(1f);
            }
            else if (!IsChanging())
            {
               transparencyChange=StartCoroutine(DoTransparencyChange(1f, durationTurnOn));
            }
        }

        IEnumerator DoTransparencyChange(float alpha, float duration) {
            float currentAlpha = Group.alpha;
            float currentTime = 0f;

            m_isFading = true;
            SetInteractable(blockRaysDuringFade);
            duration = duration * Mathf.Abs(alpha - currentAlpha);

            while (currentTime <= duration) {
                Group.alpha = Mathf.Lerp(currentAlpha, alpha, currentTime/duration);
                currentTime += Time.deltaTime;
                yield return null;
            }

            m_isFading = false;
            SetAlpha(alpha);
    
        }

        public void SetAlpha(float alpha) {
            transparencyChange = null;
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
