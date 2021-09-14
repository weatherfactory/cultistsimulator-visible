#pragma warning disable 0649
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Assets.Scripts.Application.Spheres;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;

using Random = UnityEngine.Random;

namespace SecretHistories.UI {
    public abstract class AbstractDetailsWindow : 
        NavigationAnimation, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,ISettingSubscriber {

        [SerializeField] protected TextMeshProUGUI title;
        [SerializeField] protected TextMeshProUGUI description;
        
        [SerializeField] protected CanvasGroupFader fader;

        [Header("Image")]
		[SerializeField] protected Image artwork;
		[SerializeField] protected Image artworkPin;

        public float baseInfoTimer = 10f;
        public float baseTimePerNotch = 2f;

        float waitTime = 10f;
        float time;

        private bool _isHoveringOver;


        protected void Show()
        {
            ResetTimer();
            if(fader.IsVisible())
            {
                UpdateContent();
                var args = new NavigationArgs(0, NavigationAnimationDirection.None, NavigationAnimationDirection.MoveRight);
                TriggerAnimation(args);
            }
            else
            {
                var args = new NavigationArgs(0, NavigationAnimationDirection.MoveRight, NavigationAnimationDirection.MoveRight);
                args.OnBegin = UpdateContentAfterNavigation;

                TriggerAnimation(args);
            }
        }


        public void Start()
        {
            var notificationTimeSetting = Watchman.Get<Compendium>().GetEntityById<Setting>(NoonConstants.NOTIFICATIONTIME);
            if (notificationTimeSetting == null)
            { NoonUtility.Log("Missing setting entity: " + NoonConstants.NOTIFICATIONTIME);
                return;
            }

            notificationTimeSetting.AddSubscriber(this);
            WhenSettingUpdated(notificationTimeSetting.CurrentValue);


        }

        public void WhenSettingUpdated(object newValue)
        {
            var timeInSeconds = GetInspectionTimeForValue((newValue is float ? (float)newValue : 0));
            SetTimer(timeInSeconds);
        }


        private void SetTimer(float time) {
            waitTime = time;
        }


        float GetInspectionTimeForValue(float value)
        {
            return baseInfoTimer + value * baseTimePerNotch;
        }

        public void ResetTimer() {
            // Resets the time for the deactivation whenever content is updated
            time = 0f;
        }


        protected abstract void UpdateContentAfterNavigation(NavigationArgs args);


        // Used as a delegate when exchanging pages. 
        protected abstract void UpdateContent();

        // Used as a delegate when closing pages. 
        protected abstract void ClearContent();

        protected void ShowText(string header, string desc) {
            title.text = header;
            description.text = desc;
        }

        protected void ShowImage(Sprite image) {
			artworkPin.gameObject.SetActive(image != null);
            artwork.gameObject.SetActive(image != null);
            artwork.sprite = image;
            artwork.transform.localEulerAngles = new Vector3(0f, 0f, -5f + Random.value * 10f);
        }

        IEnumerator DoWaitForHide() {
            while (time < waitTime) {
                if (!IsBusy() && !_isHoveringOver)
                    time += Time.deltaTime;

                yield return null;
            }

            Hide();
        }

        public void Hide() {
            if(fader.IsVisible())
            {
                var args = new NavigationArgs(0, NavigationAnimationDirection.MoveRight, NavigationAnimationDirection.None);
                args.OnEnd = DoHideAfterNavigation;
                TriggerAnimation(args);
            }

        }

        void DoHideAfterNavigation(NavigationArgs args)
        {
            DoHide();
        }


        void DoHide() {
           fader.Hide();
            ClearContent();
        }

        public void OnPointerClick(PointerEventData eventData) {
            Hide();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHoveringOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHoveringOver = false;
        }


    }
}
