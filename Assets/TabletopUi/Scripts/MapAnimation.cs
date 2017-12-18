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

        [SerializeField] Image background;
        [SerializeField] CanvasZoomTest zoom;

        public void Init() {
            gameObject.SetActive(false); // turn off at start
        }

        public bool CanShow(bool showMap) {
            return !isAnimating && gameObject.activeSelf != showMap;
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