using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    public class MapAnimation : MonoBehaviour {

        public event System.Action<bool> onAnimDone;

        public Color colorBeforeShow = new Color(0f, 1f, 0f, 1f);
        public float durationShow = 2f;
        public Color colorVisible = new Color(1f, 1f, 1f, 1f);
        public float durationHide = 1f;
        public Color colorAfterHidden = new Color(1f, 1f, 1f, 0f);

        bool isAnimating;
        const string uvTexName = "_FadeTex";

        [SerializeField] Image background;
        [SerializeField] ParticleSystem particles;
        [SerializeField] CanvasZoomTest zoom;

        float particleTargetRadius;

        public void Init() {
            gameObject.SetActive(false); // turn off at start
        }

        public bool CanShow(bool showMap) {
            return !isAnimating && gameObject.activeSelf != showMap;
        }

        public void SetCenterForEffect(Transform effectOrigin) {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(background.canvas.worldCamera, effectOrigin.position);
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(background.rectTransform, screenPoint, background.canvas.worldCamera, out localPoint);

            localPoint.x /= background.rectTransform.rect.width;
            localPoint.y /= background.rectTransform.rect.height;
            localPoint += background.rectTransform.pivot;

            Debug.Log("screen point " + screenPoint + " / Local point " +localPoint);

            particles.transform.position = effectOrigin.position;
            SetMaterialCenter(localPoint);
        }

        void SetMaterialCenter(Vector2 center) {
            var scaleX = 1f / (1f + Mathf.Abs(0.5f - center.x) * 2f);
            var scaleY = 1f / (1f + Mathf.Abs(0.5f - center.y) * 2f);

            var uvScale = new Vector2(scaleX, scaleY);
            var uvOffset = new Vector2(Mathf.Max(0, 1f - scaleX), Mathf.Min(0, 1f - scaleY));

            particleTargetRadius = Mathf.Max(1f / uvScale.x, 1f / uvScale.y);

            background.material.SetTextureOffset(uvTexName, uvOffset);
            background.material.SetTextureScale(uvTexName, uvScale);
        }

        public void Show(bool showMap) {
            gameObject.SetActive(true);

            if (showMap) 
                StartCoroutine(DoAnim(durationShow, colorBeforeShow, colorVisible, true));
            else
                StartCoroutine(DoAnim(durationHide, colorVisible, colorAfterHidden, false));
        }

        IEnumerator DoAnim(float duration, Color colorA, Color colorB, bool shownAtEnd) {
            float time = 0f;
            zoom.SetTargetZoom(1f);
            isAnimating = true;

            while (time < duration) {
                time += Time.deltaTime;
                background.color = Color.Lerp(colorA, colorB, time / duration);

                if (shownAtEnd) {
                    var shp = particles.shape;
                    shp.radius = Mathf.Lerp(0f, particleTargetRadius, time / duration * 1.5f);
                }

                yield return null;
            }

            background.color = colorB;
            gameObject.SetActive(shownAtEnd);
            isAnimating = false;

            if (onAnimDone != null)
                onAnimDone(shownAtEnd);
        }

    }
}