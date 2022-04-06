using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Constants;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Ghosts;
using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.Manifestations
{
    public class EdictManifestation: BasicManifestation,IManifestation

    {
        [SerializeField] Image artwork;


        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI sup;

        [Header("Countdown")]
        [SerializeField] GameObject countdownCanvas;
        [SerializeField] Image countdownBar;
        [SerializeField] Image countdownBadge;
        [SerializeField] TextMeshProUGUI countdownText;


        [Header("Ongoing Slot")]
        [SerializeField]
        Image ongoingSlotImage;

        [SerializeField] Image ongoingSlotArtImage;
        [SerializeField] GameObject ongoingSlotGreedyIcon;

        [SerializeField] public GraphicFader glowImage;



        private List<Sprite> frames;
        private Coroutine animCoroutine;

        public bool RequestingNoDrag { get; }
        public bool RequestingNoSplit => true;

        private bool _spontaneousVerb; //separate bool because I don't want an explicit reference to Verb, I want to keep it as IManifestable. But there's already a hack below using illuminations
        //to make this possible. So, once we have verb bookcases and whatnot, this perhaps should be refactored to something less special-case, like a strategy class to govern
        //temp vs permie vs other behaviour

#pragma warning restore 649


        public void Initialise(IManifestable manifestable)
        {
            string art = manifestable.Icon;

            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(art);
            frames = ResourcesManager.GetAnimFramesForVerb(art);
            artwork.sprite = sprite;
            
            SetInitialTimerVisuals();

            UpdateVisuals(manifestable);

        }

        public void UpdateVisuals(IManifestable manifestable)
        {
            var timeshadow = manifestable.GetTimeshadow();
            
            if (timeshadow.Transient)
            {
                UpdateTimerVisuals(timeshadow.Lifetime, timeshadow.LifetimeRemaining, timeshadow.LastInterval, timeshadow.Resaturate, timeshadow.EndingFlavour);
            }

            TryOverrideVerbIcon(manifestable.GetAspects(true));
            DisplayRecipeThreshold(manifestable);
            sup.text = manifestable.Label;
            EndIfComplete(manifestable);
        }

        private void EndIfComplete(IManifestable manifestable)
        {

        }


        private void TryOverrideVerbIcon(AspectsDictionary forAspects)
        {
            //if we have an element in the situation now that overrides the verb icon, update it
            string overrideIcon = Watchman.Get<Compendium>().GetVerbIconOverrideFromAspects(forAspects);
            if (!string.IsNullOrEmpty(overrideIcon))
            {
                OverrideIcon(overrideIcon);
                // _window.DisplayIcon(overrideIcon);
            }
        }


        private void SetInitialTimerVisuals()
        {
            UpdateTimerVisuals(0f, 0f, 0f, false, EndingFlavour.None);
        }

        private void UpdateTimerVisuals(float originalDuration, float durationRemaining, float interval, bool resaturate,
            EndingFlavour signalEndingFlavour)
        {
            if (durationRemaining > 0.0f)
            {
                SetTimerVisibility(true);


                durationRemaining = Mathf.Max(0f, durationRemaining);

                countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (durationRemaining / originalDuration));
                countdownText.text = Watchman.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage(durationRemaining);
                countdownText.richText = true;
            }
            else
            {
                SetTimerVisibility(false);
            }
        }

        protected void OnDisable()
        {
            // this resets any animation frames so we don't get stuck when deactivating mid-anim
            ResetIconAnimation();

        }

        public void SendNotification(INotification notification)
        {
            //
        }

        public void OverrideIcon(string art)
        {

            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(art);
            frames = ResourcesManager.GetAnimFramesForVerb(art);
            artwork.sprite = sprite;
        }

        public Vector3 GetOngoingSlotPosition()
        {
            return ongoingSlotImage.rectTransform.anchoredPosition3D;
        }



        public void DoMove(RectTransform tokenRectTransform)
        {
            shadow.DoMove(tokenRectTransform);
        }


        public bool HandlePointerClick(PointerEventData eventData, Token token)
        {

                return false;
        }

        private void DisplayRecipeThreshold(IManifestable manifestable)
        {
            var recipeThresholdDominion = manifestable.Dominions.SingleOrDefault(d =>
                d.Identifier == SituationDominionEnum.RecipeThresholds.ToString());

            if (recipeThresholdDominion == null)
                return;

            var recipeThresholdSpheres = recipeThresholdDominion.Spheres;

            if (recipeThresholdSpheres.Count == 0)
                HideMiniSlot();

            if (!recipeThresholdSpheres.Any())
                HideMiniSlot();
            else
            {
                var sphereToDisplayAsMiniSlot = recipeThresholdSpheres.Single();
                ShowMiniSlot(sphereToDisplayAsMiniSlot.GoverningSphereSpec.Greedy);
                displayStackInMiniSlot(sphereToDisplayAsMiniSlot.GetElementTokens());
            }

        }



        public IGhost CreateGhost()
        {
            var newGhost = Watchman.Get<PrefabFactory>()
                .CreateGhostPrefab(typeof(VerbGhost), this.RectTransform);
            return newGhost;
        }

        public OccupiesSpaceAs OccupiesSpaceAs() => Enums.OccupiesSpaceAs.Intangible;


        private void HideMiniSlot()
        {
            ongoingSlotImage.gameObject.SetActive(false);
            ongoingSlotGreedyIcon.gameObject.SetActive(false);

        }

        private void ShowMiniSlot(bool greedy)
        {

            if (!ongoingSlotImage.isActiveAndEnabled)
            {
                ongoingSlotImage.gameObject.SetActive(true);

            }
            if (greedy)
                ongoingSlotGreedyIcon.gameObject.SetActive(true);
            else
                ongoingSlotGreedyIcon.gameObject.SetActive(false);

        }


        private void displayStackInMiniSlot(IEnumerable<Token> tokens)
        {
            if (tokens.Count() > 1)
            {
                NoonUtility.LogWarning("EdictManifestation implementation doessn't support >1 stack in minislot");
                return;
            }


            var token = tokens.SingleOrDefault();
            if (token == null)
            {
                ongoingSlotArtImage.sprite = null;
                ongoingSlotArtImage.color = Color.black;
            }
            else
            {
                ElementStack elementStackLordForgiveMe = token.Payload as ElementStack;
                ongoingSlotArtImage.sprite = ResourcesManager.GetSpriteForElement(elementStackLordForgiveMe?.Icon);
                ongoingSlotArtImage.color = Color.white;
            }
        }




        private void SetTimerVisibility(bool show)
        {
            countdownCanvas.gameObject.SetActive(show);
        }



        public override void Retire(RetirementVFX vfx, Action callbackOnRetired)
        {
            Destroy(gameObject);
            callbackOnRetired();
        }



        public void ResetIconAnimation()
        {
            artwork.overrideSprite = null;
        }



        public void BeginIconAnimation()
        {
            if (!CanAnimateIcon())
                return;

            if (animCoroutine != null)
                StopCoroutine(animCoroutine);

            //verb animations are long-duration!
            float duration = 0.8f;
            int frameCount = frames.Count;
            int frameIndex = 0;

            animCoroutine = StartCoroutine(DoAnim(duration, frameCount, frameIndex));
        }

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

        public bool CanAnimateIcon()
        {
            return frames.Any();
        }



        private void SetGlowColor(Color color)
        {
            glowImage.SetColor(color);
        }

        private void SetGlowColor(UIStyle.GlowPurpose purposeType)
        {
            SetGlowColor(UIStyle.GetGlowColor(purposeType, UIStyle.GlowTheme.Classic));
        }

        private void ShowGlow(bool glowState, bool instant = false)
        {

            if (glowState)
                glowImage.Show(instant);
            else
                glowImage.Hide(instant);
        }

        private void ShowHoverGlow(bool show)
        {

            if (show)
            {
                SoundManager.PlaySfx("TokenHover");

                glowImage.SetColor(UIStyle.GetGlowColor(UIStyle.GlowPurpose.OnHover, UIStyle.GlowTheme.Classic));
                glowImage.Show();
            }
            else
            {

                glowImage.Hide();
            }
        }

        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
            if (highlightType == HighlightType.WillInteract)
            {
                SetGlowColor(UIStyle.GlowPurpose.Default);
                ShowGlow(true, false);
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
            else
                NoonUtility.Log("Verb anchor isn't sure what to do with highlight type " + highlightType);

        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
            if (highlightType == HighlightType.All)
            {
                ShowHoverGlow(false);
                ShowGlow(false);
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
            else
                NoonUtility.Log("Verb anchor isn't sure what to do with highlight type " + highlightType);

        }

        public bool NoPush { get; }
        public void Unshroud(bool instant)
        {
            //you never know when it might be useful
            canvasGroup.alpha = 1f;
        }

        public void Shroud(bool instant)
        {
            //you never know when it might be useful
            canvasGroup.alpha = 0.1f;
        }

        public void Emphasise()
        {


        }

        public void Understate()
        {


        }


    }
}
