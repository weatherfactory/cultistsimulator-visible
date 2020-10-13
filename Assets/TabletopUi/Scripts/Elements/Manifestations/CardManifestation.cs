using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using Noon;
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

        private Image decayBackgroundImage;
        private Color cachedDecayBackgroundColor;
        private CardVFX retirementVfx = CardVFX.CardBurn;


        public void DisplayVisuals(Element element)
        {
            Sprite sprite = ResourcesManager.GetSpriteForElement(element.Icon);
            artwork.sprite = sprite;

            if (sprite == null)
                artwork.color = Color.clear;
            else
                artwork.color = Color.white;

            SetCardBackground(element.Unique, element.Decays);

            name = "Card_" + element.Id;
            decayBackgroundImage = decayView.GetComponent<Image>();
            cachedDecayBackgroundColor = decayBackgroundImage.color;

        }

       private void SetCardBackground(bool unique, bool decays)
        {
            if (unique)
                textBackground.overrideSprite = spriteUniqueTextBG;
            else if (decays)
                textBackground.overrideSprite = spriteDecaysTextBG;
            else
                textBackground.overrideSprite = null;
        }

        public void UpdateText(Element element, int quantity)
        {
            text.text = element.Label;
            stackBadge.gameObject.SetActive(quantity > 1);
            stackCountText.text = quantity.ToString();

        }

        public void ResetAnimations()
        {
            artwork.overrideSprite = null;
        }

        public void SetVfx(CardVFX vfx)
        {
            //room here to specify the vfx type / change to args
            retirementVfx = vfx;
        }

        public bool Retire(CanvasGroup canvasGroup)
        {
            if (retirementVfx == CardVFX.CardHide || retirementVfx == CardVFX.CardHide)
            {
                StartCoroutine( FadeCard(canvasGroup,0.5f));
            }
            else
            {
                // Check if we have an effect
                CardEffectRemove effect;

                if (retirementVfx == CardVFX.None || !gameObject.activeInHierarchy)
                    effect = null;
                else
                    effect = InstantiateEffect(retirementVfx.ToString());

                if (effect != null)
                    effect.StartAnim(this.transform);
                else
                    Destroy(gameObject);
            }

            return true;
        }

        private CardEffectRemove InstantiateEffect(string effectName)
        {
            var prefab = Resources.Load("FX/RemoveCard/" + effectName);

            if (prefab == null)
                return null;

            var obj = Instantiate(prefab) as GameObject;

            if (obj == null)
                return null;

            return obj.GetComponent<CardEffectRemove>();
        }

        private IEnumerator FadeCard(CanvasGroup canvasGroup, float fadeDuration)
        {
            float time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = 1f - time / fadeDuration;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
