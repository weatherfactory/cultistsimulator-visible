#pragma warning disable 0649
using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace Assets.CS.TabletopUI {
    public class ElementStackBadge : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField] ElementStackToken token;
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
            if (!token.Defunct && DraggableToken.itemBeingDragged == null)
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
