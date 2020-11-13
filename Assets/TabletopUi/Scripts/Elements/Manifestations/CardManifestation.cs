using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Elements
{
    public class CardManifestation : MonoBehaviour, IManifestation
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
        [SerializeField] public BasicShadowImplementation shadow;
        [SerializeField] public CanvasGroup canvasGroup;
        [SerializeField] public GraphicFader glowImage;

     
        private Image decayBackgroundImage;
        private Color cachedDecayBackgroundColor;
        private bool decayVisible = false;
        private float decayAlpha = 0.0f;
        private Coroutine animCoroutine;
        private List<Sprite> frames;
        private FlipHelper flipHelper;

        public bool RequestingNoDrag => flipHelper.FlipInProgress;


        public void InitialiseVisuals(Element element)
        {
            flipHelper = new FlipHelper(this);
            Sprite sprite = ResourcesManager.GetSpriteForElement(element.Icon);
            artwork.sprite = sprite;

            if (sprite == null)
                artwork.color = Color.clear;
            else
                artwork.color = Color.white;

            SetCardBackground(element.Unique, element.Decays);

            name = "CardManifestation_" + element.Id;
            decayBackgroundImage = decayView.GetComponent<Image>();
            cachedDecayBackgroundColor = decayBackgroundImage.color;

            frames = ResourcesManager.GetAnimFramesForElement(element.Id);

        }

        public void InitialiseVisuals(IVerb verb)
        {
            throw new NotImplementedException();
        }

        public void UpdateTimerVisuals(float duration, float timeRemaining, EndingFlavour signalEndingFlavour)
        {
            throw new NotImplementedException();
        }

        public void UpdateTimerVisuals(float originalDuration, float durationRemaining,float interval,bool resaturate,EndingFlavour signalEndingFlavour)
        {

            string cardDecayTime =
                Registry.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage(durationRemaining);
            decayCountText.text = cardDecayTime;
            decayCountText.richText = true;

            // Decide whether timer should be visible or not
            if (durationRemaining < originalDuration / 2)
                ShowCardDecayTimer(true);
            

            // This handles moving the alpha value towards the desired target
            float cosmetic_dt =
                Mathf.Max(interval, Time.deltaTime) *
                2.0f; // This allows us to call AdvanceTime with 0 delta and still get animation
            if (decayVisible)
                decayAlpha = Mathf.MoveTowards(decayAlpha, 1.0f, cosmetic_dt);
            else
                decayAlpha = Mathf.MoveTowards(decayAlpha, 0.0f, cosmetic_dt);
            if (durationRemaining <= 0.0f)
                decayAlpha = 0.0f;
            if (decayView && decayView.gameObject)
            {
                decayView.gameObject.SetActive(decayAlpha > 0.0f);
            }

            // Set the text and background alpha so it fades on and off smoothly
            if (decayCountText && decayBackgroundImage)
            {
                Color col = decayCountText.color;
                col.a = decayAlpha;
                decayCountText.color = col;
                col = cachedDecayBackgroundColor; // Caching the color so that we can multiply with the non-1 alpha - CP
                col.a *= decayAlpha;
                decayBackgroundImage.color = col;
            }

            float percentageDecayed = 1 - durationRemaining / originalDuration;
                percentageDecayed = Mathf.Clamp01(percentageDecayed);
                if (resaturate)
                {
                    float reversePercentage = 1f - percentageDecayed;
                    artwork.color = new Color(1f - reversePercentage, 1f - reversePercentage, 1f - reversePercentage, 1f);
                }
                else
                {
                    artwork.color = new Color(1f - percentageDecayed, 1f - percentageDecayed, 1f - percentageDecayed, 1f);
                }



        }

        public void SendNotification(INotification notification)
        {
            NoonUtility.LogWarning("CardManifestation doesn't support SendNotification");
        }

        public void Emphasise()
        {
            canvasGroup.alpha = 1f;
        }

        public void Understate()
        {
            canvasGroup.alpha = 0.3f;
        }






        public bool HandleClick(PointerEventData eventData, Token token)
        {
       return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
            NoonUtility.LogWarning("CardManifestation doesn't support DisplaySpheres");
        }

        public void OverrideIcon(string icon)
        {
            NoonUtility.LogWarning("CardManifestation doesn't support OverrideIcon");
        }


        public void SetParticleSimulationSpace(Transform transform)
        {
            NoonUtility.LogWarning("CardManifestation doesn't support OverrideIcon");
        }

        public void AnimateTo(IArtAnimatableToken token, float duration, Vector3 startPos, Vector3 endPos, Action<VerbAnchor> SituationAnimDone,
            float startScale = 1, float endScale = 1)
        {
            NoonUtility.LogWarning("CardManifestation doesn't support AnimateTo(but perhaps it should)");
        }

        public void OnBeginDragVisuals()
        {
            ShowCardShadow(true); // Ensure we always have a shadow when dragging
            
        }


        public void OnEndDragVisuals()
        {
            ShowCardShadow(false);

        }

        public void Highlight(HighlightType highlightType)
        {
            if (highlightType == HighlightType.CanMerge)
            {
                SetGlowColor(UIStyle.TokenGlowColor.Default);
                ShowGlow(true,false);
            }
            else if (highlightType == HighlightType.AttentionPls)
            {
                SetGlowColor(UIStyle.TokenGlowColor.Default);
                StartCoroutine(PulseGlow());
            }
            else if (highlightType == HighlightType.CanInteractWithOtherToken)
            {
                ShowHoverGlow(true, false, UIStyle.brightPink);
            }
            else if (highlightType == HighlightType.Hover)
            {
                ShowHoverGlow(true);
            }

        }

        public void Unhighlight(HighlightType highlightType)
        {
        if (highlightType == HighlightType.Hover)
        {
            ShowHoverGlow(false);
        }
        else if (highlightType == HighlightType.CanInteractWithOtherToken)
        {
            ShowHoverGlow(false, false);
        }
        else if(highlightType==HighlightType.CanMerge || highlightType==HighlightType.CanFitSlot)
            ShowGlow(false,false);
        }

        private IEnumerator PulseGlow()
        {
            ShowHoverGlow(true, false, Color.white);
            yield return new WaitForSeconds(0.5f);
            ShowHoverGlow(false);
        }

        public void DoMove(RectTransform tokenRectTransform)
        {
           shadow.DoMove(tokenRectTransform);
        }


        private void ShowGlow(bool glowState, bool instant)
        {
            if (glowState)
                glowImage.Show(instant);
            else
                glowImage.Hide(instant);
        }

        private void ShowCardShadow(bool show)
        {
            shadow.gameObject.SetActive(show);
        }


        private void ShowCardDecayTimer(bool showTimer)
        {
            if (decayView != null)
                decayView.gameObject.SetActive(showTimer);
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

        public void UpdateVisuals(Element element, int quantity)
        {
            text.text = element.Label;
            stackBadge.gameObject.SetActive(quantity > 1);
            stackCountText.text = quantity.ToString();

        }


        private bool IsGlowing()
        {
            if (glowImage == null)
                return false;
            return glowImage.gameObject.activeSelf;
        }

        private void SetGlowColor(Color color)
        {
            glowImage.SetColor(color);
        }

        private void SetGlowColor(UIStyle.TokenGlowColor colorType)
        {
            SetGlowColor(UIStyle.GetGlowColor(colorType));
        }

        private void ShowHoverGlow(bool show, bool playSFX = true, Color? hoverColor = null)
        {
            
            if (show)
            {
                if (playSFX)
                    SoundManager.PlaySfx("TokenHover");

                glowImage.SetColor(hoverColor == null ? UIStyle.GetGlowColor(UIStyle.TokenGlowColor.OnHover) : hoverColor.Value);
                glowImage.Show();
            }
            else
            {
                //if (playSFX)
                //    SoundManager.PlaySfx("TokenHoverOff");
                glowImage.Hide();
            }
        }


        public bool NoPush
        {
            get { return false; }
        }

        public void Reveal(bool instant)
        {
            if (!instant)
                SoundManager.PlaySfx("CardTurnOver");

            flipHelper.Flip(FlipHelper.TargetOrientation.FaceUp,instant);


        }

        public void Shroud(bool instant)
        {
            flipHelper.Flip(FlipHelper.TargetOrientation.FaceDown, instant);
        }

        


        public void Retire(RetirementVFX retirementVfx,Action callbackOnRetired)
        {
            
            if (retirementVfx == RetirementVFX.CardHide)
            {
                StartCoroutine( FadeCard(this.gameObject.GetComponentInParent<CanvasGroup>(),0.5f));
            }
            else
            {
                // Check if we have an effect
                CardEffectRemove effect;

                if (retirementVfx == RetirementVFX.None || !gameObject.activeInHierarchy)
                    effect = null;
                else
                    effect = InstantiateEffect(retirementVfx.ToString());

                if (effect != null)
                    effect.StartAnim(this.transform);
                else
                    Destroy(gameObject);
            }

            callbackOnRetired();
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

        private void SetBackface(string backId)
        {
            Sprite sprite;

            if (string.IsNullOrEmpty(backId))
                sprite = null;
            else
                sprite = ResourcesManager.GetSpriteForCardBack(backId);

            backArtwork.overrideSprite = sprite;
        }

        public void ResetAnimations()
        {
            artwork.overrideSprite = null;
            // we're turning? Just set us to the target
            flipHelper.FinishFlip();
        }

        public bool CanAnimate()
        {
            return frames.Any();
        }

        public void BeginArtAnimation()
        {
            if (animCoroutine != null)
                StopCoroutine(animCoroutine);
      

            float duration = 0.2f;
            int frameCount = frames.Count;
            int frameIndex = 0;

            animCoroutine = StartCoroutine(DoAnim(duration, frameCount, frameIndex));
        }

        /// <param name="duration">Determines how long the animation runs. Time is spent equally on all frames</param>
        /// <param name="frameCount">How many frames to show. Default is 1</param>
        /// <param name="frameIndex">At which frame to start. Default is 0</param>
        private IEnumerator DoAnim(float duration, int frameCount, int frameIndex)
        {


            float time = 0f;
            int lastSpriteIndex = -1;

            while (time < duration)
            {
                time += Time.deltaTime;
                int spriteIndex;
                if (frameCount == 1)
                    spriteIndex = 0;
                else
                    spriteIndex = Mathf.FloorToInt(time / duration * frameCount);


                if (spriteIndex != lastSpriteIndex)
                {
                    lastSpriteIndex = spriteIndex;
                    if (spriteIndex < frames.Count)
                    {
                        artwork.overrideSprite = frames[spriteIndex];
                    }
                    else
                        artwork.overrideSprite = null;
                }
                yield return null;
            }

            // remove anim
            artwork.overrideSprite = null;
        }



    }
}
