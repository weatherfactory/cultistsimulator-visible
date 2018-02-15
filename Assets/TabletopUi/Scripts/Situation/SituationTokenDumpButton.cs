using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.CS.TabletopUI {
    public class SituationTokenDumpButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField] Image buttonImg;
        [SerializeField] GraphicFader glowImage;
        [SerializeField] GraphicFader iconImage;

        [SerializeField] Color buttonColorDefault;
        [SerializeField] Color buttonColorHover;

        public void Show(bool show) {
            gameObject.SetActive(show);
            ShowGlow(false, true);
        }

        public void OnPointerClick(PointerEventData eventData) {
            Debug.Log("Clicked DumpButton");
        }

        public void OnPointerEnter(PointerEventData eventData) {
            ShowGlow(true);
        }

        public void OnPointerExit(PointerEventData eventData) {
            ShowGlow(false);
        }

        public void ShowGlow(bool glowState, bool instant = false) {
            if (glowState) {
                buttonImg.color = buttonColorHover;
                glowImage.Show(instant);
                iconImage.SetColor(UIStyle.lightBlue);
            }
            else {
                buttonImg.color = buttonColorDefault;
                iconImage.SetColor(UIStyle.hoverWhite);
                glowImage.Hide(instant);
            }
        }
    }
}