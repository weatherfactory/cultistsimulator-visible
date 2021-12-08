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

        public bool IsVisible()
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
            if (IsInvisible())
                return;

            if (_appearingFlag && _appearingCoroutine != null)
                StopCoroutine(_appearingCoroutine);

            if (durationTurnOff <= 0f) {
                SetFinalAlpha(0f);
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
            SetFinalAlpha(0f);
        }

        public void ShowImmediately()
        {
            SetFinalAlpha(1f);
        }

        public void Show()
        {
            //show / hide repeatedly still seems to get stuck sometimes.
            //it may be worth another look to behave differently if this gets called repeatedly in successionW

            if (IsVisible())
                return;


            //I commented this out because it seems to be implicated in the stuttering effect when windows were opening?
            //if(_disappearingCoroutine!=null)
            //    StopCoroutine(_disappearingCoroutine);


            if (durationTurnOn <= 0f) {
                SetFinalAlpha(1f);
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

            SetFinalAlpha(targetAlpha);

            if (_onChangeComplete != null)
            {
                _onChangeComplete();
                _onChangeComplete = null;
            }
    
        }

        public void SetFinalAlpha(float alpha) {
            if(_appearingCoroutine!=null)
                StopCoroutine(_appearingCoroutine);
       
            if (_disappearingCoroutine != null)
                StopCoroutine(_disappearingCoroutine);

            _appearingFlag = false;
            _disappearingFlag = false;

            Group.alpha = alpha;

            if (Mathf.Approximately(alpha, 0f))
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
