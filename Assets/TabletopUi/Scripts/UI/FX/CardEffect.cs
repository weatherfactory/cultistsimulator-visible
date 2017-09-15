using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CS.TabletopUI {
    public abstract class CardEffect : MonoBehaviour {

        protected CanvasGroup targetCard;

        public virtual void StartAnim(ElementStackToken card) {
            targetCard = card.GetComponent<CanvasGroup>();
            targetCard.interactable = false;
            targetCard.blocksRaycasts = false;

        }

        public virtual void OnAnimDone() {
            if (targetCard != null)
                Destroy(targetCard.gameObject);
        }

        public void OnDisable() {
            OnAnimDone();
        }
    }
}