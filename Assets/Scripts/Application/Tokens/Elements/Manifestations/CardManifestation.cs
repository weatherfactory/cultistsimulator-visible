﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Elements;
using SecretHistories.Ghosts;
using SecretHistories.Services;
using SecretHistories.Spheres;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.Manifestations
{
    [RequireComponent(typeof(RectTransform))]
    public class CardManifestation : MonoBehaviour, IManifestation,IPointerEnterHandler,IPointerExitHandler,IBeginDragHandler
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


        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform=> gameObject.GetComponent<RectTransform>();
     
        private Image decayBackgroundImage;
        private Color cachedDecayBackgroundColor;
        private bool decayVisible = false;
        private float decayAlpha = 0.0f;
        private Coroutine animCoroutine;
        private List<Sprite> frames;
        private FlipHelper _flipHelper;
        private string _entityId;
        private int _quantity;

        public bool RequestingNoDrag => _flipHelper.FlipInProgress;
        

        public void Awake()
        {
            _flipHelper = new FlipHelper(this);
        }
        
        public void InitialiseVisuals(IManifestable manifestable)
        {
            
               Sprite sprite = ResourcesManager.GetSpriteForElement(manifestable.Icon);
            artwork.sprite = sprite;

            if (sprite == null)
                artwork.color = Color.clear;
            else
                artwork.color = Color.white;

            SetCardBackground(manifestable.Unique, manifestable.GetTimeshadow().Transient);

            name = "CardManifestation_" + manifestable.Id;
            decayBackgroundImage = decayView.GetComponent<Image>();
            cachedDecayBackgroundColor = decayBackgroundImage.color;

            frames = ResourcesManager.GetAnimFramesForElement(manifestable.EntityId);
            _entityId = manifestable.EntityId;
            _quantity = manifestable.Quantity;

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
        

        public bool HandlePointerDown(PointerEventData eventData, Token token)
        {
       return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
            NoonUtility.LogWarning("CardManifestation doesn't support DisplaySpheres");
        }

        public IGhost CreateGhost()
        {
            var newGhost = Watchman.Get<PrefabFactory>()
                .CreateGhostPrefab(typeof(CardGhost), this.RectTransform);
            return newGhost;
        }

        public void OverrideIcon(string icon)
        {
            NoonUtility.LogWarning("CardManifestation doesn't support OverrideIcon");
        }


        public void SetParticleSimulationSpace(Transform transform)
        {
            NoonUtility.LogWarning("CardManifestation doesn't support OverrideIcon");
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
            if (highlightType == HighlightType.WillInteract)
            {
                SetGlowColor(UIStyle.TokenGlowColor.Default);
                ShowGlow(true,false);
            }
            else if (highlightType == HighlightType.AttentionPls)
            {
                SetGlowColor(UIStyle.TokenGlowColor.Default);
                StartCoroutine(PulseGlow());
            }
            else if (highlightType == HighlightType.PotentiallyRelevant)
            {
                SetGlowColor(UIStyle.brightPink);
                ShowGlow(true, false);
            }
            else if (highlightType == HighlightType.Hover)
            {
                ShowHoverGlow(true);
            }

        }

        public void Unhighlight(HighlightType highlightType)
        {
            if(highlightType==HighlightType.All)
            {
                ShowGlow(false,false);
                ShowHoverGlow(false);
                return;
            }

            if (highlightType == HighlightType.Hover)
            {
                ShowHoverGlow(false);
            }
            else if (highlightType == HighlightType.PotentiallyRelevant || highlightType == HighlightType.WillInteract)
            {
                ShowGlow(false, false);
            }
        
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
            if(glowImage.Equals(null))
                return; //just in case this manifestation is currently undergoing Destroy

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

        public void UpdateVisuals(IManifestable manifestable)
        {
            _entityId = manifestable.EntityId;
            _quantity = manifestable.Quantity;

            text.text = manifestable.Label;
            stackBadge.gameObject.SetActive(_quantity > 1);
            stackCountText.text = _quantity.ToString();
            var timeshadow = manifestable.GetTimeshadow();
            UpdateTimerVisuals(timeshadow.Lifetime,timeshadow.LifetimeRemaining,timeshadow.LastInterval,timeshadow.Resaturate);

        }


        private void UpdateTimerVisuals(float originalDuration, float durationRemaining, float interval, bool resaturate)
        {

            if (originalDuration <= 0) //this card doesn't decay: never mind the rest
                return;

            string cardDecayTime =
                Watchman.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage(durationRemaining);

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


        private bool IsGlowing()
        {
            if (glowImage == null)
                return false;
            return glowImage.gameObject.activeSelf;
        }

        private void SetGlowColor(Color color)
        {
            if(glowImage.currentColor!=color)

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

        public void Unshroud(bool instant)
        {
            if (!instant)
                SoundManager.PlaySfx("CardTurnOver");

            _flipHelper?.Flip(FlipHelper.TargetOrientation.FaceUp,instant);
            
        }

        public void Shroud(bool instant)
        {
            _flipHelper.Flip(FlipHelper.TargetOrientation.FaceDown, instant);
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

        protected void OnDisable()
        {
            // this resets any animation frames so we don't get stuck when deactivating mid-anim
            ResetIconAnimation();
        }

        public void ResetIconAnimation()
        {
            artwork.overrideSprite = null;
            // we're turning? Just set us to the target
            _flipHelper.FinishFlip();
        }

        public bool CanAnimateIcon()
        {
            return frames.Any();
        }

        public void BeginIconAnimation()
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

        public void OnPointerEnter(PointerEventData eventData)
        {

            var tabletopManager = Watchman.Get<TabletopManager>();
            if (tabletopManager != null
            ) //eg we might have a face down card on the credits page - in the longer term, of course, this should get interfaced
            {
                if (_flipHelper.CurrentOrientation!=FlipHelper.TargetOrientation.FaceDown)
                    tabletopManager.SetHighlightedElement(_entityId,_quantity);
                else
                    tabletopManager.SetHighlightedElement(null);
            }

            //ExecuteEvents.Execute<IPointerEnterHandler>(transform.parent.gameObject, eventData,
            //    (parentToken, y) => parentToken.OnPointerEnter(eventData));
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            var ttm = Watchman.Get<TabletopManager>();
            if (ttm != null)
            {
                Watchman.Get<TabletopManager>().SetHighlightedElement(null);
            }

            ExecuteEvents.Execute<IPointerExitHandler>(transform.parent.gameObject, eventData,
                (parentToken, y) => parentToken.OnPointerExit(eventData));
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            NoonUtility.Log("CardManifestation begindrag");
            //ExecuteEvents.Execute<IBeginDragHandler>(transform.parent.gameObject, eventData,
            //    (parentToken, y) => parentToken.OnBeginDrag(eventData));
        }
    }
    }
