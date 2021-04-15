#pragma warning disable 0649
using System;
using System.Collections.Generic;
using SecretHistories.Core;
using SecretHistories.Fucine;
using SecretHistories.UI.Scripts;
using SecretHistories.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using SecretHistories.Constants;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace SecretHistories.UI {
    public class ElementStackBadge : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IBeginDragHandler {

        [SerializeField] Image image;
        [SerializeField] Sprite badgeHoverSprite;

        public bool PointerAboveThis { get; protected set; }


        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerAboveThis = true;
            // only highlight if we're not dragging anything
            if (!eventData.dragging)
            {
                image.overrideSprite = badgeHoverSprite;
                SoundManager.PlaySfx("TokenHover");
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            PointerAboveThis = false;
            image.overrideSprite = null;
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            NoonUtility.Log("StackBadge begindrag");
        }
    }
}
