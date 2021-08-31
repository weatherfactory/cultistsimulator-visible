using System;
using System.Collections;
using SecretHistories.Tokens.TokenPayloads;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.UI {
    public class OtherworldTransitionAnimation : OtherworldTransitionFX {

        public Color colorBeforeShow = new Color(0f, 1f, 0f, 1f);
        public float durationShow = 2f;
        public Color colorVisible = new Color(1f, 1f, 1f, 1f);
        public float durationHide = 1f;
        public Color colorAfterHidden = new Color(1f, 1f, 1f, 0f);
        private event System.Action onAnimationComplete;

        bool isAnimating;
        const string uvTexName = "_FadeTex";
#pragma warning disable 649
        [SerializeField] Image background;
#pragma warning restore 649

 

        public override bool CanShow() {

            
            return !isAnimating && gameObject.activeSelf != true;
        }

        public override bool CanHide()
        {

            return !isAnimating && gameObject.activeSelf != false;
        }


        public void SetCenterForEffect(Transform effectOrigin) {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(background.canvas.worldCamera, effectOrigin.position);
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(background.rectTransform, screenPoint, background.canvas.worldCamera, out localPoint);

            localPoint.x /= background.rectTransform.rect.width;
            localPoint.y /= background.rectTransform.rect.height;
            localPoint += background.rectTransform.pivot;

            //  Debug.Log("screen point " + screenPoint + " / Local point " +localPoint);

            SetMaterialCenter(localPoint);
        }

        void SetMaterialCenter(Vector2 center) {
            var scaleX = 1f / (1f + Mathf.Abs(0.5f - center.x) * 2f);
            var scaleY = 1f / (1f + Mathf.Abs(0.5f - center.y) * 2f);

            var uvScale = new Vector2(scaleX, scaleY);
            var uvOffset = new Vector2(Mathf.Max(0, 1f - scaleX), Mathf.Min(0, 1f - scaleY));


            background.material.SetTextureOffset(uvTexName, uvOffset);
            background.material.SetTextureScale(uvTexName, uvScale);
        }

        public override void Show(Ingress activeIngress, Action onShowComplete)
        {
            
            gameObject.SetActive(true);
            onAnimationComplete = onShowComplete;
            StartCoroutine(DoAnimation(durationShow, colorBeforeShow, colorVisible, true));
            SoundManager.PlaySfx(EntrySfxName);
            Watchman.Get<BackgroundMusic>().PlayOtherworldClip(Music);

        }


        public override void Hide(Action onHideComplete)
        {
            onAnimationComplete += onHideComplete;
            StartCoroutine(DoAnimation(durationHide, colorVisible, colorAfterHidden, false));
            SoundManager.PlaySfx(ExitSfxName);
        }

        IEnumerator DoAnimation(float duration, Color colorA, Color colorB, bool shownAtEnd) {
            float time = 0f;
            isAnimating = true;

            while (time < duration) {
                time += Time.deltaTime;
                background.color = Color.Lerp(colorA, colorB, time / duration);

     

                yield return null;
            }

            background.color = colorB;
            gameObject.SetActive(shownAtEnd);
            isAnimating = false;

            

            if (onAnimationComplete != null)
                onAnimationComplete();

            onAnimationComplete = null;
            
        }

    }
}