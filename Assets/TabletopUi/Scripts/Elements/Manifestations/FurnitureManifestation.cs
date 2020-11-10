using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
    public class FurnitureManifestation : MonoBehaviour, IAnchorManifestation
    {
        [SerializeField] Image artwork;

        [Header("Token Body")]
        [SerializeField] Image tokenBody;
        [SerializeField] Sprite lightweightSprite;
        [SerializeField] private CanvasGroup canvasGroup;


        [Header("Countdown")]
        [SerializeField] GameObject countdownCanvas;
        [SerializeField] Image countdownBar;
        [SerializeField] Image countdownBadge;
        [SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] ParticleSystem[] particles;

        [Header("Ongoing Slot")]
        [SerializeField]
        Image ongoingSlotImage;

        [SerializeField] Image ongoingSlotArtImage;
        [SerializeField] GameObject ongoingSlotGreedyIcon;
        [SerializeField] ParticleSystem ongoingSlotAppearFX;

        [Header("Completion")]
        [SerializeField]
        Image completionBadge;

        [SerializeField] TextMeshProUGUI completionText;

        [Header("DumpButton")]
        [SerializeField]
        SituationTokenDumpButton dumpButton;

        [SerializeField] public GraphicFader glowImage;

        private List<Sprite> frames;
        private bool _transient;
        private Coroutine animCoroutine;



        public void Awake()
        {
            ongoingSlotImage.gameObject.SetActive(false);
        }

        public void InitialiseVisuals(IVerb verb)
        {
            displayIcon(verb.Id);
            if (verb.Transient)
                SetTransient();
            SetTimerVisibility(false);
            SetCompletionCount(-1);
            ShowGlow(false, false);
            ShowDumpButton(false);
        }

        public void OverrideIcon(string icon)
        {
            displayIcon(icon);
        }

        public Vector3 GetOngoingSlotPosition()
        {
            return ongoingSlotImage.rectTransform.anchoredPosition3D;
        }

        public void DoMove(RectTransform tokenRectTransform)
        {
            
        }

        public void SetCompletionCount(int newCount)
        {
            // count == -1 ? No badge
            // count ==  0 ? badge, no text
            // count >=  1 ? badge and text

            completionBadge.gameObject.SetActive(newCount >= 0);
            completionText.gameObject.SetActive(newCount > 0);
            completionText.text = newCount.ToString();

            ShowDumpButton(newCount >= 0);
        }

        public void ReceiveAndRefineTextNotification(INotification notification)
        {
            //do nothing
        }

        public bool HandleClick(PointerEventData eventData, VerbAnchor anchor)
        {
            return false;
        }

        public void ShowMiniSlot(bool greedy)
        {

            if (!ongoingSlotImage.isActiveAndEnabled)
            {
                ongoingSlotImage.gameObject.SetActive(true);

                ongoingSlotAppearFX.Play();
                SoundManager.PlaySfx("SituationTokenShowOngoingSlot");
                if (greedy)
                    ongoingSlotGreedyIcon.gameObject.SetActive(true);
            }

        }

        public void HideMiniSlot()
        {
            ongoingSlotImage.gameObject.SetActive(false);
            ongoingSlotGreedyIcon.gameObject.SetActive(false);

        }

        public void DisplayStackInMiniSlot(ElementStackToken stack)
        {

            if (stack == null)
            {
                ongoingSlotArtImage.sprite = null;
                ongoingSlotArtImage.color = Color.black;
            }
            else
            {
                ongoingSlotArtImage.sprite = ResourcesManager.GetSpriteForElement(stack.Icon);
                ongoingSlotArtImage.color = Color.white;
            }
        }

        public void DisplayComplete()
        {
            SetTimerVisibility(false);
            HideMiniSlot();
        }


        public void UpdateTimerVisuals(float duration, float timeRemaining, EndingFlavour signalEndingFlavour)
        {
            if (timeRemaining > 0.0f)
            {
                SetTimerVisibility(true);

                Color barColor = UIStyle.GetColorForCountdownBar(signalEndingFlavour, timeRemaining);

                timeRemaining = Mathf.Max(0f, timeRemaining);
                countdownBar.color = barColor;
                countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (timeRemaining / duration));
                countdownText.color = barColor;
                countdownText.text = Registry.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage(timeRemaining);
                countdownText.richText = true;
            }
            else
            {
                SetTimerVisibility(false);
            }
        }

        private void SetTransient()
        {
            _transient = true;
            tokenBody.overrideSprite = lightweightSprite;

        }


        private void SetTimerVisibility(bool show)
        {
            // If we're changing the state, change the particles
            if (show != countdownCanvas.gameObject.activeSelf)
            {
                if (show)
                    particles[0].Play(); // only need to hit play on the first one
                else
                    particles[0].Stop();
            }

            countdownCanvas.gameObject.SetActive(show);
        }

        private void displayIcon(string icon)
        {
            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(icon);
            frames = ResourcesManager.GetAnimFramesForVerb(icon);
            artwork.sprite = sprite;
        }

        private void ShowDumpButton(bool showButton)
        {
            dumpButton.gameObject.SetActive(showButton && _transient);
        }

        public void ResetAnimations()
        {
            throw new NotImplementedException();
        }

        public void Retire(RetirementVFX vfx, Action callbackOnRetired)
        {
            Destroy(gameObject);
            callbackOnRetired();
        }


        public void BeginArtAnimation()
        {
            if (!CanAnimate())
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

        public bool CanAnimate()
        {
            return frames.Any();
        }

        public void OnBeginDragVisuals()
        {

        }

        public void OnEndDragVisuals()
        {

        }

        private void SetGlowColor(Color color)
        {
            glowImage.SetColor(color);
        }

        private void SetGlowColor(UIStyle.TokenGlowColor colorType)
        {
            SetGlowColor(UIStyle.GetGlowColor(colorType));
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

                glowImage.SetColor(UIStyle.GetGlowColor(UIStyle.TokenGlowColor.OnHover));
                glowImage.Show();
            }
            else
            {

                SoundManager.PlaySfx("TokenHoverOff");
                glowImage.Hide();
            }
        }

        public void Highlight(HighlightType highlightType)
        {
            if (highlightType == HighlightType.CanInteractWithOtherToken)
            {
                SetGlowColor(UIStyle.TokenGlowColor.Default);
                ShowGlow(true, false);
            }
            else if (highlightType == HighlightType.Hover)
            {
                ShowHoverGlow(true);
            }
            else
                NoonUtility.Log("Verb anchor isn't sure what to do with highlight type " + highlightType);

        }

        public void Unhighlight(HighlightType highlightType)
        {
            if (highlightType == HighlightType.Hover)
            {
                ShowHoverGlow(false);
            }
            else if (highlightType == HighlightType.CanInteractWithOtherToken)
            {
                ShowGlow(false, false);
            }
            else
                NoonUtility.Log("Verb anchor isn't sure what to do with highlight type " + highlightType);

        }

        public bool NoPush { get; }
        public void DoRevealEffect(bool instant)
        {
            throw new NotImplementedException();
        }

        public void DoShroudEffect(bool instant)
        {
            throw new NotImplementedException();
        }

        public bool RequestingNoDrag { get; }


        /// <summary>
        /// needs to be set to initial token container
        /// </summary>
        /// <param name="transform"></param>
        public void SetParticleSimulationSpace(Transform transform)
        {
            ParticleSystem.MainModule mainSettings;

            for (int i = 0; i < particles.Length; i++)
            {
                mainSettings = particles[i].main;
                mainSettings.customSimulationSpace = transform;
            }
        }


        public void AnimateTo(IArtAnimatableToken token, float duration, Vector3 startPos, Vector3 endPos,
            Action<VerbAnchor> SituationAnimDone, float startScale = 1f, float endScale = 1f)
        {
            var tokenAnim = token.gameObject.AddComponent<TokenAnimation>();
            tokenAnim.onAnimDone += SituationAnimDone;
            tokenAnim.SetPositions(startPos, endPos);
            tokenAnim.SetScaling(startScale, endScale);
            tokenAnim.StartAnim(duration);
        }

    }
}
