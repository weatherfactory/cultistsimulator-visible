#pragma warning disable 0649
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Assets.Core.Entities;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using Random = UnityEngine.Random;

namespace Assets.CS.TabletopUI {
    public abstract class BaseDetailsWindow : 
        AnimatedNoteBase, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,ISettingSubscriber {

        [SerializeField] protected TextMeshProUGUI title;
        [SerializeField] protected TextMeshProUGUI description;

        [Header("Image")]
		[SerializeField] protected Image artwork;
		[SerializeField] protected Image artworkPin;

        public float baseInfoTimer = 10f;
        public float baseTimePerNotch = 2f;

        float waitTime = 10f;
        float time;

        private bool _isHoveringOver;

        protected void Show() {
            ResetTimer();

            if (gameObject.activeSelf == false) {
                // Show the content, then make the anim move in
                gameObject.SetActive(true);
                UpdateContent();
                TriggerAnim(AnimType.None, AnimType.MoveRight);
                StartCoroutine(DoWaitForHide());
            }
            else {
                // Make the anim move out, then show the content, then move in again
                TriggerAnim(AnimType.MoveRight, AnimType.MoveRight, UpdateContent);
            }
        }

        public void Start()
        {
            var notificationTimeSetting = Registry.Get<ICompendium>().GetEntityById<Setting>(NoonConstants.NOTIFICATIONTIME);
            if (notificationTimeSetting == null)
            { NoonUtility.Log("Missing setting entity: " + NoonConstants.NOTIFICATIONTIME);
                return;
            }

            notificationTimeSetting.AddSubscriber(this);
            UpdateValueFromSetting(notificationTimeSetting.CurrentValue);
        }

        public void UpdateValueFromSetting(object newValue)
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
			if (gameObject.activeInHierarchy) {
				TriggerAnim(AnimType.MoveRight, AnimType.None, DoHide);
			}
        }

        void DoHide() {
            gameObject.SetActive(false);
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
