using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CS.TabletopUI {
    public abstract class CardEffect : MonoBehaviour {

        protected CanvasGroup targetCard;

        public abstract void StartAnim(ElementStackToken card);
        public abstract void OnAnimDone();

        public virtual void OnDisable() {
            OnAnimDone();
        }
    }
}