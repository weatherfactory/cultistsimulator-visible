using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    [ExecuteInEditMode]
    [RequireComponent(typeof(Graphic))]
    public class GraphicFader : MonoBehaviour {

        public bool ignoreTimeScale = true;
        public float durationTurnOn = 0.2f;
        public float durationTurnOff = 0.1f;
        public Color currentColor = Color.white;

        Graphic m_graphic;
        Graphic graphic {
            get {
                if (m_graphic == null) { 
                    m_graphic = GetComponent<Graphic>();
                }

                return m_graphic;
            }
        }

        public void Hide(bool instant = false) {
            StopAllCoroutines();

            if (instant || graphic.gameObject.activeInHierarchy == false || durationTurnOn <= 0f) {
                SetAlpha(0f);
                return;
            }

            if (graphic.canvasRenderer.GetAlpha() > 0f) {
                graphic.CrossFadeAlpha(0f, durationTurnOff, ignoreTimeScale);
                StartCoroutine(DelayDisable(durationTurnOff));
            }
        }

        IEnumerator DelayDisable(float duration) {
            yield return new WaitForSeconds(duration);
            graphic.gameObject.SetActive(false);
        }

        public void Show(bool instant = false) {
            StopAllCoroutines();

            if (instant || durationTurnOn <= 0f) {
                graphic.gameObject.SetActive(true);
                SetAlpha(1f);
                return;
            }

            if (graphic.gameObject.activeSelf == false) {
                graphic.gameObject.SetActive(true);
                graphic.canvasRenderer.SetColor(currentColor);
                graphic.canvasRenderer.SetAlpha(0f);
            }

            if (graphic.canvasRenderer.GetAlpha() < 1f) {
                graphic.CrossFadeAlpha(1f, durationTurnOn, ignoreTimeScale);
            }
        }

        public void SetAlpha(float alpha) {
            if (Mathf.Approximately(alpha, 0f))
                graphic.gameObject.SetActive(false);
            else
                graphic.canvasRenderer.SetAlpha(alpha);
        }

        public void SetColor(Color color) {
            if (currentColor == color)
                return;

            currentColor = color;

            if (graphic.gameObject.activeInHierarchy)
                graphic.canvasRenderer.SetColor(color);
        }

        #if UNITY_EDITOR
        // Ensure we can see the color in the editor.
        void OnEnable() {
            graphic.canvasRenderer.SetColor(currentColor);
        }
        void OnValidate() {
            graphic.canvasRenderer.SetColor(currentColor);
        }
        // Copies the graphic's current color to the Fader and sets the image tint to white
        void Reset() {
            currentColor = graphic.color;
            graphic.color = Color.white;
            graphic.canvasRenderer.SetColor(currentColor);
        }
        #endif

    }
}
