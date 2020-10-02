using UnityEngine;
using System.Collections;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Assets.TabletopUi.Scripts.Infrastructure {
    public class EndGameAnimController : MonoBehaviour {
#pragma warning disable 649
        [SerializeField] Vector2 targetPosOffset = new Vector2(0f, -150f);

        [Header("Controllers")]
        [SerializeField] private TabletopManager _tabletopManager;
        [SerializeField] private SpeedControlUI _speedControlUi;
        [SerializeField] private UIController _uiController;
        
        [Header("Visuals")]
        [SerializeField] private Canvas tableCanvas;
		[SerializeField] private CameraZoom cameraZoom;
        [SerializeField] private ScrollRect tableScroll;
        [SerializeField] private Canvas menuCanvas;
        [SerializeField] private Image fadeOverlay;
#pragma warning restore 649
        bool isEnding = false;

        public void Initialise() {
            fadeOverlay.gameObject.SetActive(false);
        }

        public void TriggerEnd(ISituationAnchor culpableVerb, Ending ending) {
            if (isEnding)
                return;

            isEnding = true;
            StartCoroutine(DoEndGameAnim(culpableVerb, ending));
        }

        IEnumerator DoEndGameAnim(ISituationAnchor culpableVerb, Ending ending) {
            const float zoomDuration = 5f;
            const float fadeDuration = 2f;

            // disable all input
            GraphicRaycaster rayCaster;
            rayCaster = tableCanvas.GetComponent<GraphicRaycaster>();
            rayCaster.enabled = false; // Disable clicks on tabletop

            rayCaster = menuCanvas.GetComponent<GraphicRaycaster>();
            rayCaster.enabled = false; // Disable clicks on Screen

			cameraZoom.enablePlayerZoom = false;
            _uiController.enabled = false; // Disable shortcuts

            // pause game
          Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel =3 , GameSpeed = GameSpeed.Paused, WithSFX =false });


            // Abort all interactions
            DraggableToken.draggingEnabled = false; // this SHOULD disable the dragging
            tableScroll.StopMovement(); // make sure the scroll rect stops
			tableScroll.movementType = ScrollRect.MovementType.Unrestricted; // this allows us to leave the boundaries on the anim in case our token is at the table edges
            _tabletopManager.CloseAllSituationWindowsExcept(null); // no window has an id of NULL, so all close

            // TODO: play death effect / music

            var menuBarCanvasGrp = menuCanvas.GetComponent<CanvasGroup>();
            float time = 0f;
            Vector2 startPos = tableScroll.content.anchoredPosition;
            Vector2 targetPos = -1f * culpableVerb.RectTransform.anchoredPosition + targetPosOffset;
            // ^ WARNING: targetPosOffset fixes the difference between the scrollable and tokenParent rect sizes 

            Debug.Log("Target Zoom Pos " + targetPos);

            // Start zoom in on offending token 
			cameraZoom.StartFixedZoom(0f, zoomDuration);

            // Start hiding all tokens
            RetireAllStacks(CardVFX.CardBurn);

            // (Spawn specific effect based on token, depending on end-game-type)
            InstantiateEffect(ending, culpableVerb.transform);

            while (time < zoomDuration && !_uiController.IsPressingAbortHotkey()) {
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

            Registry.Get<StageHand>().EndingScreen();

        }

        GameObject InstantiateEffect(Ending ending, Transform token) {

            string effectName;

            if (string.IsNullOrEmpty(ending.Anim))
                effectName = "DramaticLight";
            else
                effectName = ending.Anim;


            var prefab = Resources.Load("FX/EndGame/" + effectName);

            if (prefab == null)
                return null;

            var go = Instantiate(prefab, token) as GameObject;
            go.transform.position = token.position;
            go.transform.localScale = Vector3.one;
            go.SetActive(true);

            var effect = go.GetComponent<CardEffect>();

            //AK temporarily commented out to fix build
            if (effect != null)
                effect.StartAnim(token);

            return go;
        }

        void RetireAllStacks(CardVFX anim) {
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
