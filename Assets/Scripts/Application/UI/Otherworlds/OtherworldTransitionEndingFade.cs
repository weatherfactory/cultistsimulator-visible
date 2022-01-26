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
            const float fadeDuration = 2f;

            
            // (Spawn specific effect based on token, depending on end-game-type)
            InstantiateEffect(ending, focusOnTransform);


            //float time = 0f;
            //Vector2 startPos = tableScroll.content.anchoredPosition;
            //Vector2 targetPos = -1f * focusOnTransform.anchoredPosition + targetPosOffset;
            //// ^ WARNING: targetPosOffset fixes the difference between the scrollable and tokenParent rect sizes 


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
