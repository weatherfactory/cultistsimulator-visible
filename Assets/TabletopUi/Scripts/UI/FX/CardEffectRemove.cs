using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CS.TabletopUI {
    public abstract class CardEffectRemove : CardEffect {
        
        [SerializeField] string sfx = "CardBurn";

        public override void StartAnim(ElementStackToken card) {            
            targetCard = card.GetComponent<CanvasGroup>();
            targetCard.interactable = false;
            targetCard.blocksRaycasts = false;

            transform.SetParent(targetCard.transform);
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            gameObject.SetActive(true);

            ScaleParticlesScale();

            if (sfx != null && sfx != "")
                SoundManager.PlaySfx(sfx);
        }

        public override void OnAnimDone() {
            if (targetCard != null)
                Destroy(targetCard.gameObject);
        }

    }
}