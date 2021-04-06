using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SecretHistories.UI {
    public abstract class CardEffect : MonoBehaviour {

        protected CanvasGroup tokenCanvasGroup;
        protected Action _callbackOnComplete;
        public void SetCallbackOnComplete(Action callbackOnComplete)
        {
            _callbackOnComplete = callbackOnComplete;
        }

        public abstract void StartAnim(Transform card);

        public virtual void OnAnimDone()
        {
            _callbackOnComplete?.Invoke();
        }


        public virtual void OnDisable() {
            OnAnimDone();
        }

    }
}