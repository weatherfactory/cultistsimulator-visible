using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SecretHistories.Fucine;

namespace SecretHistories.UI {
    public class SituationTokenDumpButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
#pragma warning disable 649
        [SerializeField] Image buttonImg;
        [SerializeField] Image iconImage;

        [SerializeField] Color buttonColorDefault;
        [SerializeField] Color buttonColorHover;
#pragma warning restore 649
        public bool PointerAboveThis { get; protected set; }

        public void Show(bool show) {
            gameObject.SetActive(show);
            ShowGlow(false, true);
        }

        private void OnDisable() {
            // make sure we're no longer hovering after we hit the button
            PointerAboveThis = false; 
        }

        
        public void OnPointerEnter(PointerEventData eventData) {
            ShowGlow(true);
            PointerAboveThis = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            ShowGlow(false);
            PointerAboveThis = false;
        }

        public void ShowGlow(bool glowState, bool instant = false) {
            if (glowState) {
                buttonImg.color = buttonColorHover;
                iconImage.color = UIStyle.lightBlue;
            }
            else {
                buttonImg.color = buttonColorDefault;
                iconImage.color = UIStyle.hoverWhite;
            }
        }
    }
}