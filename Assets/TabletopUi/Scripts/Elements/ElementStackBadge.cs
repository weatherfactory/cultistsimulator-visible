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

        [SerializeField]
        ElementStackToken token;
        [SerializeField]
        Image image;
        [SerializeField]
        Sprite stackBageHoverSprite;
        [SerializeField]
        Sprite stackBageUniqueSprite;

        bool isHovering;
        bool isUnique;

        public void SetAsUnique(bool isUnique) {
            this.isUnique = isUnique;
            UnHover(); // can not hover when card is unique

            if (isUnique)
                image.overrideSprite = stackBageUniqueSprite; // If we re-enable hover this does not work, cause the override sprite is used for hover
            else
                image.overrideSprite = null;
        }

        // to check if we're dragging the badge or the stack when starting the drag
        public bool IsHovering() {
            return isHovering;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (isUnique)
                return;

            isHovering = true;

            // only highlight if we're not dragging anything
            if (!token.Defunct && DraggableToken.itemBeingDragged == null)
                image.overrideSprite = stackBageHoverSprite;
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (isUnique)
                return;

            image.overrideSprite = null;

            // We unhover later, so hover is still true in the frame in which the drag starts
            Invoke("UnHover", 0.05f);
        }

        void UnHover() {
            isHovering = false;
        }
    }
}
