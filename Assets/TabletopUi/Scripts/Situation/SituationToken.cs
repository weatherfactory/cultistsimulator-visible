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

    public class SituationToken : DraggableToken, ISituationAnchor {

        [SerializeField] Image artwork;

        [Header("Token Body")]
        [SerializeField] Image tokenBody;
        [SerializeField] Sprite lightweightSprite;

        [Header("Countdown")]
        [SerializeField] GameObject countdownCanvas;
        [SerializeField] Image countdownBar;
        [SerializeField] Image countdownBadge;
        [SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] ParticleSystem[] particles;

        [Header("Ongoing Slot")]
        [SerializeField] Image ongoingSlotImage;
        [SerializeField] Image ongoingSlotArtImage;
        [SerializeField] GameObject ongoingSlotGreedyIcon;
		[SerializeField] ParticleSystem ongoingSlotAppearFX;

        [Header("Completion")]
        [SerializeField] Image completionBadge;
        [SerializeField] TextMeshProUGUI completionText;

        [Header("DumpButton")]
        [SerializeField] SituationTokenDumpButton dumpButton;

        [Header("Debug")]
        [SerializeField] SituationEditor situationEditor;

        private IVerb _verb;

        public SituationController SituationController {
            get; private set;
        }

        public bool IsTransient {
            get { return _verb.Transient; }
        }

        public SlotSpecification GetPrimarySlotSpecificationForVerb()
        {
            return _verb.PrimarySlotSpecification;
        }


        public bool EditorIsActive
        {
            get { return situationEditor.isActiveAndEnabled; }
        }

        public override string EntityId {
            get { return _verb == null ? null : _verb.Id; }
        }

		private bool isNew; // used for sound and SFX purposes

        public void Initialise(IVerb verb, SituationController sc, Heart heart) {
            _verb = verb;
            SituationController = sc;
            name = "Verb_" + EntityId;
			isNew = true;

            DisplayIcon(verb);
            SetAsLightweight(verb.Transient);
            SetTimerVisibility(false);
            SetCompletionCount(-1);
            ShowGlow(false, false);
            ShowDumpButton(false);

            ongoingSlotImage.gameObject.SetActive(false);
            DisplayStackInMiniSlot(null);
            situationEditor.Initialise(SituationController);
        }

        #region -- Token positioning --------------------------

        public override void ReturnToTabletop(Context context) {
            Registry.Retrieve<Choreographer>().ArrangeTokenOnTable(this, context);
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

        private void DisplayIcon(IVerb v) {
            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(v.Id);
            artwork.sprite = sprite;
        }

        private void SetAsLightweight(bool lightweight) {
            tokenBody.overrideSprite = (lightweight ? lightweightSprite : null);
        }

        public void DisplayAsOpen() {
            ShowGlow(false);
        }

        public void DisplayAsClosed() {
            ShowGlow(false);
        }

        void ShowDumpButton(bool showButton) {
            dumpButton.gameObject.SetActive(showButton && _verb.Transient);
        }

        public void SetEditorActive(bool active) {
            situationEditor.gameObject.SetActive(active);
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
            SetTimerVisibility(true);

            Color barColor = UIStyle.GetColorForCountdownBar(signalEndingFlavour, timeRemaining);

            timeRemaining = Mathf.Max(0f, timeRemaining);
            countdownBar.color = barColor;
            countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (timeRemaining / duration));
            countdownText.color = barColor;
			countdownText.text = NoonUtility.MakeTimeString( timeRemaining );
            countdownText.richText = true;
        }

        public Vector3 GetOngoingSlotPosition() {
            return RectTransform.anchoredPosition3D + ongoingSlotImage.rectTransform.anchoredPosition3D;
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


        public void DisplayStackInMiniSlot(IEnumerable<IElementStack> stacksInOngoingSlots) {
            IElementStack stack;

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

        public override void OnDrop(PointerEventData eventData) {
            if (DraggableToken.itemBeingDragged != null)
                DraggableToken.itemBeingDragged.InteractWithTokenDroppedOn(this);
        }

        public override void OnPointerClick(PointerEventData eventData) {
            if (dumpButton.IsHovering()) {
                SituationController.DumpAllResults();
            }
            else { 
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

        public override bool CanInteractWithTokenDroppedOn(IElementStack stackDroppedOn) {
            return false; // The Sit Token can't be dropped on anything
        }

        public override bool CanInteractWithTokenDroppedOn(SituationToken tokenDroppedOn) {
            return false; // The Sit Token can't be dropped on anything
        }

        public override void InteractWithTokenDroppedOn(SituationToken tokenDroppedOn) {
            bool moveAsideFor = false;
            tokenDroppedOn.TokenContainer.TryMoveAsideFor(this, tokenDroppedOn, out moveAsideFor);

            if (moveAsideFor)
                DraggableToken.SetReturn(false, "was moved aside for");
            else
                DraggableToken.SetReturn(true);
        }

        public override void InteractWithTokenDroppedOn(IElementStack stackDroppedOn) {
            bool moveAsideFor = false;
            var stackToken = stackDroppedOn as ElementStackToken;

            stackToken.TokenContainer.TryMoveAsideFor(this, stackToken, out moveAsideFor);

            if (moveAsideFor)
                DraggableToken.SetReturn(false, "was moved aside for");
            else
                DraggableToken.SetReturn(true);
        }


    }    
}
