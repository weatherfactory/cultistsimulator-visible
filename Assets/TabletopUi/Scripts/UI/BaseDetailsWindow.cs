#pragma warning disable 0649
using System.Collections.Generic;
using Assets.Core;
using Assets.TabletopUi.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace Assets.CS.TabletopUI {
    public abstract class BaseDetailsWindow : AnimatedNoteBase, IPointerClickHandler {

        [SerializeField] protected TextMeshProUGUI title;
        [SerializeField] protected TextMeshProUGUI description;

        [Header("Image")]
        [SerializeField] protected Image artwork;

        float waitTime = 10f;
        float time;

        protected void Show() {
			SoundManager.PlaySfx("InfoWindowShow");
            ResetTimer();

            if (gameObject.activeSelf == false) {
                // Show the content, then make the anim move in
                UpdateContent();
                gameObject.SetActive(true);
                TriggerAnim(AnimType.None, AnimType.MoveRight);
                StartCoroutine(DoWaitForHide());
            }
            else {
                // Make the anim move out, then show the content, then move in again
                TriggerAnim(AnimType.MoveRight, AnimType.MoveRight, UpdateContent);
            }
        }

        public void SetTimer(float time) {
            waitTime = time;
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
            artwork.gameObject.SetActive(image != null);
            artwork.sprite = image;
            artwork.transform.localEulerAngles = new Vector3(0f, 0f, -5f + Random.value * 10f);
        }

        IEnumerator DoWaitForHide() {
            while (time < waitTime) {
                if (!IsBusy())
                    time += Time.deltaTime;

                yield return null;
            }

            Hide();
        }

        public void Hide() {
            if (gameObject.activeInHierarchy)
                TriggerAnim(AnimType.MoveRight, AnimType.None, DoHide);
        }

        void DoHide() {
            gameObject.SetActive(false);
            ClearContent();
        }

        public void OnPointerClick(PointerEventData eventData) {
            // Do nothing, since we now have a dedicated "close" button
            //Hide();
        }
    }
}
