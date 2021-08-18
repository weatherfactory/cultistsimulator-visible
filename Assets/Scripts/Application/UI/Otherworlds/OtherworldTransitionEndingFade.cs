using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Tokens.TokenPayloads;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.UI

{
    //TODO:
    // disable all input.
    //below is the original code, but this may be better done through metapause
    //GraphicRaycaster rayCaster;
    //rayCaster = tableCanvas.GetComponent<GraphicRaycaster>();
    //rayCaster.enabled = false; // Disable clicks on tabletop
    //rayCaster = menuCanvas.GetComponent<GraphicRaycaster>();
    //rayCaster.enabled = false; // Disable clicks on Screen

    //cameraZoom.enablePlayerZoom = false;
    //_uiController.enabled = false; // Disable shortcuts

    // pause game
    //Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.Paused, WithSFX = false });


    //TODO: //stop scrolling.
    //tableScroll.StopMovement(); // make sure the scroll rect stops
    //tableScroll.movementType = ScrollRect.MovementType.Unrestricted; // this allows us to leave the boundaries on the anim in case our token is at the table edges

    //TODO: close all windows
    //_tabletopManager.CloseAllSituationWindowsExcept(null); // no window has an id of NULL, so all close

    // TODO: play death effect / music


    // TODO: wipe all stacks. Though we may no longer want to do this.
    // RetireAllStacks(RetirementVFX.CardBurn);


    //TODO: the actual zoom
    //float time = 0f;
    //Vector2 startPos = tableScroll.content.anchoredPosition;
    //Vector2 targetPos = -1f * focusOnTransform.anchoredPosition + targetPosOffset;
    //// ^ WARNING: targetPosOffset fixes the difference between the scrollable and tokenParent rect sizes 

    //Debug.Log("Target Zoom Pos " + targetPos);


    //cameraZoom.StartFixedZoom(0f, zoomDuration);

    //TODO: abort with abort hotkey
    //while (time < zoomDuration && !_uiController.IsPressingAbortHotkey())
    //{
    //menuBarCanvasGrp.alpha = 1f - time; // remove lower button bar.
    //tableScroll.content.anchoredPosition = Vector2.Lerp(startPos, targetPos, Easing.Circular.Out((time / zoomDuration)));
    //yield return null;
    //time += Time.deltaTime;
    //}

    // automatically jumps here on Abort - NOTE: At the moment this auto-focuses the token, but that's okay, it's important info
    //tableScroll.content.anchoredPosition = targetPos;

    //todo: make menu bar invisible
    //var menuBarCanvasGrp = menuCanvas.GetComponent<CanvasGroup>();
    //menuBarCanvasGrp.alpha = 0f;


public class OtherworldTransitionEndingFade: OtherworldTransitionFX
{
    [SerializeField] private GameObject endingContent;

    private System.Action onFadeComplete;
        public override bool CanShow()
        {
            return true;
        }

        public override bool CanHide()
        {
            return true;

        }

        public override void Show(Ingress activeIngress, Action onShowComplete)
        {
            var compendium = Watchman.Get<Compendium>();
            var ending=compendium.GetEntityById<Ending>(activeIngress.EntityId);
            RectTransform focusOnTransform = activeIngress.Token.TokenRectTransform;
            gameObject.SetActive(true);
            onFadeComplete += onShowComplete;
            StartCoroutine(DoFadeTransition(focusOnTransform,ending));
            
        }

        public override void Hide(Action onHideComplete)
        {
            //
        }

        IEnumerator DoFadeTransition(RectTransform focusOnTransform, Ending ending)
        {
            const float zoomDuration = 5f;
            const float fadeDuration = 2f;

            
            // (Spawn specific effect based on token, depending on end-game-type)
            InstantiateEffect(ending, focusOnTransform);

            
            // TODO: Put the fade into the while loop so that on aborting the zoom still continues
            Watchman.Get<TabletopFadeOverlay>().FadeToBlack(fadeDuration);
            yield return new WaitForSeconds(fadeDuration);

            onFadeComplete();
            onFadeComplete = null;
            Watchman.Get<TabletopFadeOverlay>().FadeIn(fadeDuration / 10);
            endingContent.gameObject.SetActive(true);

        }

        GameObject InstantiateEffect(Ending ending, Transform token)
        {

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
    }
}
