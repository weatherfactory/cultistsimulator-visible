
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.Elements.Manifestations
{
    [RequireComponent(typeof(RectTransform))]
    public class VerbManifestation: MonoBehaviour, IManifestation
    {
#pragma warning disable 649
        [SerializeField] Image artwork;

        [Header("Token Body")]
        [SerializeField] Image tokenBody;
        [SerializeField] Sprite lightweightSprite;
        [SerializeField] private BasicShadowImplementation shadow;
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


        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();


        private List<Sprite> frames;
        private bool _transient;
        private Coroutine animCoroutine;

#pragma warning restore 649


        public void InitialiseVisuals(IDrivesManifestation drivesManifestation)
        {

            displayArtForVerb(verb);
            if (verb.Transient)
            {
                _transient = true;
                tokenBody.overrideSprite = lightweightSprite;
                
            }
            DisplaySpheres(new List<Sphere>());
            UpdateTimerVisuals(0f, 0f, 0f, false, EndingFlavour.None);

        }

        public void UpdateVisuals(IDrivesManifestation drivesManifestation)
        {
            var timeshadow = drivesManifestation.GetTimeshadow();

            if (timeshadow.Transient)
            {
                UpdateTimerVisuals(timeshadow.LifetimeRemaining,timeshadow.LifetimeRemaining,timeshadow.LastInterval,timeshadow.Resaturate,timeshadow.EndingFlavour);
            }

            TryOverrideVerbIcon(drivesManifestation.GetAspects(true));
        }


        private void TryOverrideVerbIcon(IAspectsDictionary forAspects)
        {
            //if we have an element in the situation now that overrides the verb icon, update it
            string overrideIcon = Watchman.Get<Compendium>().GetVerbIconOverrideFromAspects(forAspects);
            if (!string.IsNullOrEmpty(overrideIcon))
            {
                OverrideIcon(overrideIcon);
               // _window.DisplayIcon(overrideIcon);
            }
        }


        private void UpdateTimerVisuals(float originalDuration, float durationRemaining, float interval, bool resaturate,
            EndingFlavour signalEndingFlavour)
        {
            if (durationRemaining > 0.0f)
            {
                SetTimerVisibility(true);

                Color barColor = UIStyle.GetColorForCountdownBar(signalEndingFlavour, durationRemaining);

                durationRemaining = Mathf.Max(0f, durationRemaining);
                countdownBar.color = barColor;
                countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (durationRemaining / originalDuration));
                countdownText.color = barColor;
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
            return  ongoingSlotImage.rectTransform.anchoredPosition3D;
        }

        public void DoMove(RectTransform tokenRectTransform)
        {
             shadow.DoMove(tokenRectTransform);
        }

  

        public bool HandlePointerDown(PointerEventData eventData, Token token)
        {
            if (dumpButton.IsHovering())
            {
                token.OnCollect.Invoke();
                return true;
            }
            else
                return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
            
            var activeThresholdSpheres = new List<Sphere>(spheres.Where(s => s.SphereCategory == SphereCategory.Threshold));
            if (!activeThresholdSpheres.Any())
                HideMiniSlot();
            else
            {
                var sphereToDisplayAsMiniSlot = activeThresholdSpheres.Single();
                ShowMiniSlot(sphereToDisplayAsMiniSlot.GoverningSphereSpec.Greedy);
                displayStackInMiniSlot(sphereToDisplayAsMiniSlot.GetElementTokens());
            }

            var outputSpheres = new List<Sphere>(spheres.Where(s => s.SphereCategory == SphereCategory.Output));
            if (outputSpheres.Any())
            {

                int completionCount = outputSpheres.Select(s => s.GetTotalElementsCount()).Sum();
                completionBadge.gameObject.SetActive(true);
                if (completionCount > 0)
                {
                    completionText.text = completionCount.ToString();
                    if(_transient)
                        dumpButton.gameObject.SetActive(true);
                    else
                        dumpButton.gameObject.SetActive(false);
                }
                else
                {
                    completionText.text = string.Empty;
                    dumpButton.gameObject.SetActive(false);
                }

            }
            else
            {
                //no active output spheres: no collection badge, no dump button
                completionBadge.gameObject.SetActive(false);
                dumpButton.gameObject.SetActive(false);
            }

        }


        private void HideMiniSlot()
        {
            ongoingSlotImage.gameObject.SetActive(false);
            ongoingSlotGreedyIcon.gameObject.SetActive(false);

        }

        private void ShowMiniSlot(bool greedy)
        {
            
            if(!ongoingSlotImage.isActiveAndEnabled)
            {
                ongoingSlotImage.gameObject.SetActive(true);

                ongoingSlotAppearFX.Play();
                SoundManager.PlaySfx("SituationTokenShowOngoingSlot");
                if(greedy)
                    ongoingSlotGreedyIcon.gameObject.SetActive(true);
            }
          
        }


        private void displayStackInMiniSlot(IEnumerable<Token> tokens)
        {
            if(tokens.Count()>1)
            {
                NoonUtility.LogWarning("VerbManifestation implementation doessn't support >1 stack in minislot");
                return;
            }
            

            var token = tokens.SingleOrDefault();
            if(token==null)
            {
                ongoingSlotArtImage.sprite = null;
                ongoingSlotArtImage.color = Color.black;
            }
            else
            {
                ElementStack elementStackLordForgiveMe=token.Payload as ElementStack;
                ongoingSlotArtImage.sprite = ResourcesManager.GetSpriteForElement(elementStackLordForgiveMe?.Icon);
                ongoingSlotArtImage.color = Color.white;
            }
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

        private void displayArtForVerb(IVerb verb)
        {
            string art;
            if (!string.IsNullOrEmpty(verb.Art))
                art = verb.Art;
            else
                art = verb.Id;

            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(art);
            frames = ResourcesManager.GetAnimFramesForVerb(art);
            artwork.sprite = sprite;
        }


        public void Retire(RetirementVFX vfx, Action callbackOnRetired)
        {
            VanishFx(vfx.ToString());
            Destroy(gameObject);
            callbackOnRetired();
        }

        private void VanishFx(string effectName)
        {

            var prefab = Resources.Load("FX/VerbAnchor/" + effectName);

            if (prefab is null)
                return;

            var vanishFxObject = Instantiate(prefab, transform.parent) as GameObject;
            if (!(vanishFxObject is null)) //I mean it shouldn't be, but let's keep the compiler happy
            {
                vanishFxObject.transform.position = transform.position;
                vanishFxObject.transform.localScale = Vector3.one;
                vanishFxObject.SetActive(true);
            }
        }


        public void ResetIconAnimation()
        {
            NoonUtility.Log("Verb manifestion ResetIcon(): use it or lose it");
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
            if(highlightType == HighlightType.CanInteractWithOtherToken)
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
        public void Reveal(bool instant)
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
            //you never know when it might be useful
            canvasGroup.alpha = 1f;

        }

        public void Understate()
        {
            //you never know when it might be useful
            canvasGroup.alpha = 0.3f;

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

   
    }
}
