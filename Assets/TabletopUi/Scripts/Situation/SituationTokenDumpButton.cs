using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.CS.TabletopUI {
    public class SituationTokenDumpButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField] Image buttonImg;
        [SerializeField] Image iconImage;

        [SerializeField] Color buttonColorDefault;
        [SerializeField] Color buttonColorHover;

        bool isHovering;

        public void Show(bool show) {
            gameObject.SetActive(show);
            ShowGlow(false, true);
        }

        private void OnDisable() {
            // make sure we're no longer hovering after we hit the button
            isHovering = false; 
        }

        // to check if we're on the dump button when clicking
        public bool IsHovering() {
            return isHovering;
        }
        
        public void OnPointerEnter(PointerEventData eventData) {
            ShowGlow(true);
            isHovering = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            ShowGlow(false);
            isHovering = false;
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