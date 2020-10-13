using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Elements
{
    public class CardManifestation: MonoBehaviour,IElementManifestation
    {

        [SerializeField] public Image artwork;
        [SerializeField] public Image backArtwork;
        [SerializeField] public Image textBackground;
        [SerializeField] public TextMeshProUGUI text;
        [SerializeField] public ElementStackBadge stackBadge;
        [SerializeField] public TextMeshProUGUI stackCountText;
        [SerializeField] public GameObject decayView;
        [SerializeField] public TextMeshProUGUI decayCountText;
        [SerializeField] public Sprite spriteDecaysTextBG;
        [SerializeField] public Sprite spriteUniqueTextBG;
        [SerializeField] public GameObject shadow;
        [SerializeField] public GraphicFader glowImage;

        public CardVFX defaultRetireFX = CardVFX.CardBurn;





        public void DisplayArt(Element element)
        {
            Sprite sprite = ResourcesManager.GetSpriteForElement(element.Icon);
            artwork.sprite = sprite;

            if (sprite == null)
                artwork.color = Color.clear;
            else
                artwork.color = Color.white;
        }

        public void DisplayInfo(Element element, int quantity)
        {
            text.text = element.Label;
            stackBadge.gameObject.SetActive(quantity > 1);
            stackCountText.text = quantity.ToString();

        }
    }
}
