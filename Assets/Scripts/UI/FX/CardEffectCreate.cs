using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CS.TabletopUI {
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class CardEffectCreate : CardEffect {

        protected CanvasGroup effectGroup;
        [SerializeField] string sfx = "CardBurn";

        public override void StartAnim(Transform token) {
            tokenCanvasGroup = token.GetComponent<CanvasGroup>();
            tokenCanvasGroup.interactable = false;
            tokenCanvasGroup.blocksRaycasts = false;
            tokenCanvasGroup.alpha = 0f;

            // Ensure we have a CanvasGroup and it ignores the parent 
            effectGroup = GetComponent<CanvasGroup>();
            effectGroup.interactable = false;
            effectGroup.blocksRaycasts = false;
            effectGroup.ignoreParentGroups = true;
            effectGroup.alpha = 1f;

            transform.SetParent(token);
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            gameObject.SetActive(true);

            if (sfx != null && sfx != "")
                SoundManager.PlaySfx(sfx);
        }

        public override void OnAnimDone() {
            if (tokenCanvasGroup != null)
                tokenCanvasGroup.alpha = 1f;

            Destroy(gameObject);
        }

    }
}