using UnityEngine;
using System.Collections;
using Assets.CS.TabletopUI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Assets.TabletopUi.Scripts.Infrastructure {
    public class EndGameAnimController : MonoBehaviour {

        [Header("Controllers")]
        [SerializeField] private TabletopManager _tabletopManager;
        [SerializeField] private SpeedController _speedController;
        [SerializeField] private HotkeyWatcher _hotkeyWatcher;
        
        [Header("Visuals")]
        [SerializeField] private Canvas tableCanvas;
        [SerializeField] private CanvasZoomTest tableZoom;
        [SerializeField] private ScrollRect tableScroll;
        [SerializeField] private Canvas menuCanvas;
        [SerializeField] private Image fadeOverlay;

        bool isEnding = false;

        public void Initialise() {
            fadeOverlay.gameObject.SetActive(false);
        }

        public void TriggerEnd(SituationToken culpableVerb, string fxName) {
            if (isEnding)
                return;

            isEnding = true;
            StartCoroutine(DoEndGameAnim(culpableVerb, fxName));
        }

        IEnumerator DoEndGameAnim(SituationToken culpableVerb, string fxName) {
            const float zoomDuration = 5f;
            const float fadeDuration = 2f;

            // disable all input
            GraphicRaycaster rayCaster;
            rayCaster = tableCanvas.GetComponent<GraphicRaycaster>();
            rayCaster.enabled = false; // Disable clicks on tabletop

            rayCaster = menuCanvas.GetComponent<GraphicRaycaster>();
            rayCaster.enabled = false; // Disable clicks on Screen

            tableZoom.enablePlayerZoom = false; // Disable player zoom control
            _hotkeyWatcher.enabled = false; // Disable shortcuts

            // pause game
            _speedController.SetPausedState(true);

            // Abort all interactions
            DraggableToken.draggingEnabled = false; // this SHOULD disable the dragging
            tableScroll.StopMovement(); // make sure the scroll rect stops
            _tabletopManager.CloseAllSituationWindowsExcept(null); // no window has an id of NULL, so all close

            Debug.Log("Target Zoom Pos " + -1f * culpableVerb.RectTransform.anchoredPosition);

            // TODO: play death effect / music

            var menuBarCanvasGrp = menuCanvas.GetComponent<CanvasGroup>();
            float time = 0f;
            Vector2 startPos = tableScroll.content.anchoredPosition;
            Vector2 targetPos = -1f * culpableVerb.RectTransform.anchoredPosition;
            // ^ WARNING: This only works since the parent RectTransform of the token has same size, pivot and pos as the content 

            // Start zoom in on offending token 
            tableZoom.StartFixedZoom(0f, zoomDuration);

            // Start hiding all tokens
            RetireAllStacks("CardBurn");

            // (Spawn specific effect based on token, depending on end-game-type)
            if (!string.IsNullOrEmpty(fxName))
                InstantiateEffect(fxName, culpableVerb.transform);

            while (time < zoomDuration && !_hotkeyWatcher.IsPressingAbortHotkey()) {
                menuBarCanvasGrp.alpha = 1f - time; // remove lower button bar.
                tableScroll.content.anchoredPosition = Vector2.Lerp(startPos, targetPos, Easing.Circular.Out((time / zoomDuration)));
                yield return null;
                time += Time.deltaTime;
            }

            // automatically jumps here on Abort - NOTE: At the moment this auto-focuses the token, but that's okay, it's important info
            menuBarCanvasGrp.alpha = 0f;
            tableScroll.content.anchoredPosition = targetPos;

            // TODO: Put the fade into the while loop so that on aborting the zoom still continues
            FadeToBlack(fadeDuration);
            yield return new WaitForSeconds(fadeDuration);

            SceneManager.LoadScene(SceneNumber.EndScene);
        }

        GameObject InstantiateEffect(string effectName, Transform parent) {
            var prefab = Resources.Load("FX/" + effectName);

            if (prefab == null)
                return null;

            var go = Instantiate(prefab, parent) as GameObject;
            go.transform.SetAsFirstSibling();
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.SetActive(true);

            return go;
        }

        void RetireAllStacks(string anim) {
            var stacks = _tabletopManager._tabletop.GetElementStacksManager().GetStacks();

            foreach (var item in stacks)
                item.Retire(anim);
        }

        public void FadeToBlack(float duration) {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.canvasRenderer.SetAlpha(0f);
            fadeOverlay.CrossFadeAlpha(1f, duration, true);
        }
    }
}
