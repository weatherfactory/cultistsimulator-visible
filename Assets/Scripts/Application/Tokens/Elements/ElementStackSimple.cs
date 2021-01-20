#pragma warning disable 0649
using System;
using System.Collections.Generic;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.UI.Scripts;
using SecretHistories.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace SecretHistories.UI {
    public class ElementStackSimple : MonoBehaviour {

        [SerializeField]
        Image artwork;
        [SerializeField]
        TextMeshProUGUI text;
        [SerializeField]
        GameObject stackBadge;
        [SerializeField]
        TextMeshProUGUI stackCountText;

        private Element _element;
        private int _quantity;

        public void Populate(string elementId, int quantity) {
            _element = Watchman.Get<Compendium>().GetEntityById<Element>(elementId);
            _quantity = quantity;

            if (_element == null)
                return;

            gameObject.name = "Card_" + elementId;
            DisplayInfo();
            DisplayIcon();
        }

        private void DisplayInfo() {
            text.text = _element.Label;
            stackBadge.gameObject.SetActive(_quantity > 1);
            stackCountText.text = _quantity.ToString();
        }

        private void DisplayIcon() {
            Sprite sprite = ResourcesManager.GetSpriteForElement(_element.Icon);
            artwork.sprite = sprite;

            if (sprite == null)
                artwork.color = Color.clear;
            else
                artwork.color = Color.white;
        }

    }
}
