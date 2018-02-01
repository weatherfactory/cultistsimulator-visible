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
    public class TokenDetailsWindow : MonoBehaviour, IPointerClickHandler {

        [SerializeField] Animation anim;
        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI description;
        [SerializeField] AspectsDisplay aspectsDisplay;

        const float waitTime = 10f;

        float time;
        bool coroutineIsStarted;

        public void Show() {
            // DO anim
            gameObject.SetActive(false);
        }

        public void SetElementCard(Element element) {
            Sprite sprite;

            if (element.IsAspect)
                sprite = ResourcesManager.GetSpriteForAspect(element.Id);
            else
                sprite = ResourcesManager.GetSpriteForElement(element.Id);

            artwork.sprite = sprite;
            title.text = element.Label;
            description.text = element.Description;
            aspectsDisplay.DisplayAspects(element.Aspects);
            time = 0f; // Resets the time for the deactivation whenever a new card is shown

            if (!coroutineIsStarted) {
                coroutineIsStarted = true;
                StartCoroutine(DoWaitForHide());
            }
        }

        public void SetSlot(RecipeSlot slot) {
        }

        IEnumerator DoWaitForHide() {
            while (time < waitTime) {
                time += Time.deltaTime;
                yield return null;
            }

            Hide();
        }

        public void Hide() {
            // DO anim
            gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData) {
            Hide();
        }
    }
}
