using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CS.TabletopUI {
    public abstract class CardEffectRemove : CardEffect {
        
        [SerializeField] string sfx = "CardBurn";

        public override void StartAnim(Transform token) {            
            tokenCanvasGroup = token.GetComponent<CanvasGroup>();
            tokenCanvasGroup.interactable = false;
            tokenCanvasGroup.blocksRaycasts = false;

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
                Destroy(tokenCanvasGroup.gameObject);
        }

    }
}