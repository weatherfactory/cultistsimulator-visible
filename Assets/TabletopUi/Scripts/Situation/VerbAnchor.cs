using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Logic;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
#pragma warning disable 0649
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.SlotsContainers;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {

    public class VerbAnchor : AbstractToken, ISituationAnchor
    {

        [SerializeField] Image artwork;

        [Header("Token Body")] [SerializeField]
        Image tokenBody;

        [SerializeField] Sprite lightweightSprite;

        [Header("Countdown")] [SerializeField] GameObject countdownCanvas;
        [SerializeField] Image countdownBar;
        [SerializeField] Image countdownBadge;
        [SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] ParticleSystem[] particles;

        [Header("Ongoing Slot")] [SerializeField]
        Image ongoingSlotImage;

        [SerializeField] Image ongoingSlotArtImage;
        [SerializeField] GameObject ongoingSlotGreedyIcon;
        [SerializeField] ParticleSystem ongoingSlotAppearFX;

        [Header("Completion")] [SerializeField]
        Image completionBadge;

        [SerializeField] TextMeshProUGUI completionText;

        [Header("DumpButton")] [SerializeField]
        SituationTokenDumpButton dumpButton;


        private string _entityid;
        private Coroutine animCoroutine;
        private List<Sprite> frames;
        private bool _transient;

        public SituationController SituationController { get; private set; }

        public bool IsTransient
        {
            get { return _transient; }
        }



        public override string EntityId
        {
            get { return _entityid; }
        }

        private bool isNew; // used for sound and SFX purposes



        public void Initialise(IVerb verb, SituationController sc) {
            
            SituationController = sc;
            _entityid = verb.Id;
            name = "Verb_" + EntityId;
			isNew = true;

            displayIcon(verb.Id);
            if(verb.Transient)
                SetTransient();
            SetTimerVisibility(false);
            SetCompletionCount(-1);
            ShowGlow(false, false);
            ShowDumpButton(false);

            ongoingSlotImage.gameObject.SetActive(false);
            DisplayStackInMiniSlot(null);
        }

        #region -- Token positioning --------------------------

        public override void ReturnToTabletop(Context context) {
            Registry.Get<Choreographer>().ArrangeTokenOnTable(this, context);
        }

        /*
        // CountdownCanvas no longer has a Canvas component - sorting was shitty
        public override void DisplayInAir() {
            base.DisplayInAir();
            countdownCanvas.overrideSorting = false;
        }

        public override void DisplayAtTableLevel() {
            base.DisplayAtTableLevel();
            countdownCanvas.overrideSorting = true;
        }
        */

        #endregion

        #region -- Token Visuals --------------------------

        public void SetParticleSimulationSpace(Transform transform) {
            ParticleSystem.MainModule mainSettings;

            for (int i = 0; i < particles.Length; i++) {
                mainSettings = particles[i].main;
                mainSettings.customSimulationSpace = transform;
            }
        }

        
        public void DisplayOverrideIcon(string icon)
        {
            displayIcon(icon);
        }

        private void displayIcon(string icon)
        {
            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(icon);
            frames = ResourcesManager.GetAnimFramesForVerb(icon);
            artwork.sprite = sprite;
        }

        private void SetTransient() {
            _transient = true;
            tokenBody.overrideSprite = lightweightSprite;
            
        }



        public override void ReactToDraggedToken(TokenInteractionEventArgs args)
        {
            if (args.TokenInteractionType == TokenInteractionType.BeginDrag)
            {

                var stack = args.Token as ElementStackToken;
                if (stack == null)
                    return;
                if (SituationController.CanAcceptStackWhenClosed(stack))
                {
                    SetGlowColor(UIStyle.TokenGlowColor.Default);
                    ShowGlow(true, false);
                }
            }
            if (args.TokenInteractionType == TokenInteractionType.EndDrag)
                ShowGlow(false,false);

        }


        public void DisplayAsOpen() {
            ShowGlow(false);
        }

        public void DisplayAsClosed() {
            ShowGlow(false);
        }

        void ShowDumpButton(bool showButton) {
            dumpButton.gameObject.SetActive(showButton && _transient);
        }


        private void SetTimerVisibility(bool show) {
            // If we're changing the state, change the particles
            if (show != countdownCanvas.gameObject.activeSelf) {
                if (show)
                    particles[0].Play(); // only need to hit play on the first one
                else
                    particles[0].Stop();
            }

            countdownCanvas.gameObject.SetActive(show);
        }

        public void DisplayTimeRemaining(float duration, float timeRemaining, EndingFlavour signalEndingFlavour)
		{
			if (timeRemaining > 0.0f)
			{
				SetTimerVisibility(true);

				Color barColor = UIStyle.GetColorForCountdownBar(signalEndingFlavour, timeRemaining);

				timeRemaining = Mathf.Max(0f, timeRemaining);
				countdownBar.color = barColor;
				countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (timeRemaining / duration));
				countdownText.color = barColor;
				countdownText.text = Registry.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage( timeRemaining );
				countdownText.richText = true;
			}
			else
			{
				SetTimerVisibility(false);
			}
        }

        public Vector3 GetOngoingSlotPosition() {
            return rectTransform.anchoredPosition3D + ongoingSlotImage.rectTransform.anchoredPosition3D;
        }

        public void DisplayMiniSlot(IList<SlotSpecification> ongoingSlots) {
            ongoingSlotImage.gameObject.SetActive(ongoingSlots != null && ongoingSlots.Count > 0);

            if (ongoingSlots == null || ongoingSlots.Count == 0)
                return;
            if (ongoingSlots.Count > 1)
                throw new InvalidOperationException("More than one ongoing slot specified for this recipe, and we don't currently know how to deal with that");

			// We're not a no-anim slot? Then show the anim!
			if (!ongoingSlots[0].NoAnim && !isNew) {
				ongoingSlotAppearFX.Play();
				SoundManager.PlaySfx("SituationTokenShowOngoingSlot");
			}

			ongoingSlotGreedyIcon.gameObject.SetActive(ongoingSlots[0].Greedy);
		}


        public void DisplayStackInMiniSlot(IEnumerable<ElementStackToken> stacksInOngoingSlots) {
            ElementStackToken stack;

            if (stacksInOngoingSlots != null)
                stack = stacksInOngoingSlots.SingleOrDefault(); //THERE CAN BE ONLY ONE (currently)
            else
                stack = null;

            if (stack == null) {
                ongoingSlotArtImage.sprite = null;
                ongoingSlotArtImage.color = Color.black;
            }
            else {
                ongoingSlotArtImage.sprite = ResourcesManager.GetSpriteForElement(stack.Icon);
                ongoingSlotArtImage.color = Color.white;
            }
        }

        public void DisplayComplete() {
            SetTimerVisibility(false);
            DisplayMiniSlot(null);
			isNew = false;
        }

        public void SetCompletionCount(int newCount) {
            // count == -1 ? No badge
            // count ==  0 ? badge, no text
            // count >=  1 ? badge and text

            completionBadge.gameObject.SetActive(newCount >= 0);
            completionText.gameObject.SetActive(newCount > 0);
            completionText.text = newCount.ToString();

            ShowDumpButton(newCount >= 0);
        }


        protected override void NotifyChroniclerPlacedOnTabletop()
        {
            //currently, we never tell chroniclers about verb placement
        }

        public override bool Retire() {
            if (!Defunct)
                SpawnKillFX();

            return base.Retire();
        }

        void SpawnKillFX() {
            var prefab = Resources.Load("FX/SituationToken/SituationTokenVanish");

            if (prefab == null)
                return;

            var go = Instantiate(prefab, transform.parent) as GameObject;
            go.transform.position = transform.position;
            go.transform.localScale = Vector3.one;
            go.SetActive(true);
        }

        #endregion


            // None of this should do view changes here. We're deferring to the SitController or TokenContainer

        public override void OnDrop(PointerEventData eventData)
        {

            InteractWithTokenDroppedOn(eventData.pointerDrag);
        }

        public override void OnPointerClick(PointerEventData eventData) {
            if (dumpButton.IsHovering()) {
                SituationController.DumpAllResults();
            }
            else {
                // Add the current recipe name, if any, to the debug panel if it's active
                Registry.Get<DebugTools>().SetInput(SituationController.Situation.RecipeId);

                if (!SituationController.IsOpen)
                    OpenSituation();
                else
                    CloseSituation();
            }
        }

        public void OpenSituation() {
            SituationController.OpenWindow();
        }

        void CloseSituation() {
            SituationController.CloseWindow();
        }

        public override bool CanInteractWithTokenDroppedOn(ElementStackToken stackDroppedOn) {
            //element stack dropped on verb - FIXED
            return SituationController.CanAcceptStackWhenClosed(stackDroppedOn);
        }

        public override void InteractWithTokenDroppedOn(ElementStackToken stackDroppedOn)
        {
            //element stack dropped on verb - FIXED
            if (CanInteractWithTokenDroppedOn(stackDroppedOn))
            {
                // This will put it into the ongoing or the starting slot, token determines
                SituationController.PushDraggedStackIntoToken(stackDroppedOn);

                // Then we open the situation (cause this closes other situations and this may return the stack we try to move
                // back onto the tabletop - if it was in its starting slots. - Martin
                if (!SituationController.IsOpen)
                    OpenSituation();
                else
                    DisplayAsOpen(); // This will turn off any unneeded hover effects
                return;
            }

            // We can't interact? Then dump us on the tabletop
            SetReturn(false, "Tried to drop on non-compatible token, return to tabletop");
            ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));

            
        }

        public override bool CanInteractWithTokenDroppedOn(VerbAnchor tokenDroppedOn) {
            //verb dropped on verb - OK
            return false; // a verb anchor can't be dropped on anything
        }

        public override void InteractWithTokenDroppedOn(VerbAnchor tokenDroppedOn) {
            //verb dropped on verb - OK
            
            tokenDroppedOn.TokenContainer.TryMoveAsideFor(this, tokenDroppedOn, out bool moveAsideFor);

            if (moveAsideFor)
                SetReturn(false, "was moved aside for");
            else
                SetReturn(true);
        }




        /// Trigger an animation on the card
        /// </summary>
        /// <param name="duration">Determines how long the animation runs. Time is spent equally on all frames</param>
        /// <param name="frameCount">How many frames to show. Default is 1</param>
        /// <param name="frameIndex">At which frame to start. Default is 0</param>
        public override void StartArtAnimation()
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

        public override IEnumerator DoAnim(float duration, int frameCount, int frameIndex)
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

        public override bool CanAnimate()
        {
            if (gameObject == null)
                return false;

            if (gameObject.activeInHierarchy == false)
                return false; // can not animate if deactivated

            return frames.Any();
        }
    }
}
