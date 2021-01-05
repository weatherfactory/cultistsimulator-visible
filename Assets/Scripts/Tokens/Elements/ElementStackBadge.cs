#pragma warning disable 0649
using System;
using System.Collections.Generic;
using SecretHistories.Core;
using SecretHistories.Interfaces;
using SecretHistories.UI.Scripts;
using SecretHistories.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using SecretHistories.Infrastructure;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace SecretHistories.UI {
    public class ElementStackBadge : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField] Image image;
        [SerializeField] Sprite badgeHoverSprite;

        bool isHovering;

        // to check if we're dragging the badge or the stack when starting the drag
        public bool IsHovering() {
            return isHovering;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            isHovering = true;

			SoundManager.PlaySfx("TokenHover");

            // only highlight if we're not dragging anything
            if (!eventData.dragging)
                image.overrideSprite = badgeHoverSprite;
            
        }

        public void OnPointerExit(PointerEventData eventData) {
            image.overrideSprite = null;

            // We unhover later, so hover is still true in the frame in which the drag starts
            Invoke("UnHover", 0.05f);
        }

        void UnHover() {
            isHovering = false;
        }
    }
}
