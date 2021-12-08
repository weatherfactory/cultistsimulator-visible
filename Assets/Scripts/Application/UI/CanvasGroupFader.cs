using System;
using System.Collections;
using Assets.Scripts.Application.Fucine;
using UnityEngine;

namespace SecretHistories.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupFader : MonoBehaviour {

        public bool blockRaysDuringFade = false;

        public float durationTurnOn;
        public float durationTurnOff;

        private Coroutine _appearingCoroutine;
        private Coroutine _disappearingCoroutine;
        private bool _appearingFlag = false;
        private bool _disappearingFlag = false;

        private Action _onChangeComplete;

        public bool IsFullyVisible()
        {
            return Group.alpha >=1f;
        }

        public bool IsInvisible()
        {
            return Group.alpha <= 0f;
        }


        private CanvasGroup Group => gameObject.GetComponent<CanvasGroup>();
    

        public void SetOnChangeCompleteCallback(Action onChangeComplete)
        {
            _onChangeComplete = onChangeComplete;
        }

        public void Hide()
        {
            //if we're currently trying to appear, shut that down
            if (_appearingFlag && _appearingCoroutine != null)
                StopCoroutine(_appearingCoroutine);

            //if we're already invisible or if we're hiding instantly, set the final alpha state.
            //This will ensure things like interactable are tidied to the invisible state if we got stuck halfway through
            if (durationTurnOff <= 0f && IsInvisible()) {
                SetStatesForFinalAlpha(0f);
            }
            else
            {
                if(!_disappearingFlag)
                {
                    _disappearingFlag = true;
                    _appearingFlag = false; //probably unnecessary but this is racey world
                    _disappearingCoroutine = StartCoroutine(DoTransparencyChange(0f, durationTurnOff));
                    
                }
            }
                
        }

        public void HideImmediately()
        {
            SetStatesForFinalAlpha(0f);
        }

        public void ShowImmediately()
        {
            SetStatesForFinalAlpha(1f);
        }

        public void Show()
        {
            //show / hide repeatedly still seems to get stuck sometimes.
            //it may be worth another look to behave differently if this gets called repeatedly in successionW
            
            if (_disappearingFlag && _disappearingCoroutine != null)
                StopCoroutine(_disappearingCoroutine);

            //If we're appearing instantly or if we are already visible, go to final alpha state, make sure everything is set correctly for that.
            if (durationTurnOn <= 0f && IsFullyVisible()) {
                SetStatesForFinalAlpha(1f);
            }
            else
            {
                if(!_appearingFlag)
                {
                    _appearingFlag = true;
                    _disappearingFlag = false; //probably unnecessary but this is racey world
                    _appearingCoroutine = StartCoroutine(DoTransparencyChange(1f, durationTurnOn));
                }
            }
        }

        IEnumerator DoTransparencyChange(float targetAlpha, float duration) {
            float currentAlpha = Group.alpha;
            float currentTime = 0f;

            SetInteractable(blockRaysDuringFade); 
            duration = duration * Mathf.Abs(targetAlpha - currentAlpha);

            while (currentTime <= duration) {
                Group.alpha = Mathf.Lerp(currentAlpha, targetAlpha, currentTime/duration);
                currentTime += Time.deltaTime;
                yield return null;
            }

            SetStatesForFinalAlpha(targetAlpha);

            if (_onChangeComplete != null)
            {
                _onChangeComplete();
                _onChangeComplete = null;
            }
    
        }

        public void SetStatesForFinalAlpha(float finalAlpha) {
            if(_appearingCoroutine!=null)
                StopCoroutine(_appearingCoroutine);
       
            if (_disappearingCoroutine != null)
                StopCoroutine(_disappearingCoroutine);

            _appearingFlag = false;
            _disappearingFlag = false;

            Group.alpha = finalAlpha;

            if (Mathf.Approximately(finalAlpha, 0f))
                SetInteractable(false);
            else
                SetInteractable(true);
        }

        void SetInteractable(bool state) {
            Group.blocksRaycasts = state;
            Group.interactable = state;
        }

    }
}
