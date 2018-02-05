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

        [SerializeField] TextMeshProUGUI text;
        [SerializeField] Canvas countdownCanvas;
        [SerializeField] Image countdownBar;
        [SerializeField] Image countdownBadge;
        [SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] Image completionBadge;
        [SerializeField] TextMeshProUGUI completionText;
        [SerializeField] private SituationEditor situationEditor;

        [SerializeField] Image ongoingSlotImage;
        [SerializeField] Image ongoingSlotArtImage;

        private IVerb _verb;

        public SituationController SituationController {
            get; private set;
        }

        public bool IsTransient {
            get { return _verb.Transient; }
        }

        public bool EditorIsActive
        {
            get { return situationEditor.isActiveAndEnabled; }
        }

        public override string Id {
            get { return _verb == null ? null : _verb.Id; }
        }

        public void Initialise(IVerb verb, SituationController sc) {
            _verb = verb;
            SituationController = sc;
            name = "Verb_" + Id;

            DisplayName(verb);
            DisplayIcon(verb);
            SetTimerVisibility(false);
            SetCompletionCount(0);
            ShowGlow(false, false);

            ongoingSlotImage.gameObject.SetActive(false);
           situationEditor.Initialise(SituationController);
        }

        #region -- Token positioning --------------------------

        public override void ReturnToTabletop(INotification reason = null) {
            Registry.Retrieve<Choreographer>().ArrangeTokenOnTable(this);

            if (reason != null)
                notifier.ShowTokenReturnToTabletopNotification(this, reason);
        }

        public override void DisplayInAir() {
            base.DisplayInAir();
            countdownCanvas.overrideSorting = false;
        }

        public override void DisplayAtTableLevel() {
            base.DisplayAtTableLevel();
            countdownCanvas.overrideSorting = true;
        }

        #endregion

        #region -- Token Visuals --------------------------

        private void DisplayName(IVerb v) {
            text.text = v.Label;
        }

        private void DisplayIcon(IVerb v) {
            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(v.Id);
            artwork.sprite = sprite;
        }

        public void DisplayAsOpen() {
            ShowGlow(false);
        }

        public void DisplayAsClosed() {
            ShowGlow(false);
        }

        public void SetEditorActive(bool active)
        {
            situationEditor.gameObject.SetActive(active);
        }

        

        private void SetTimerVisibility(bool b) {
            countdownCanvas.gameObject.SetActive(b);
        }

        public void DisplayTimeRemaining(float duration, float timeRemaining, Recipe recipe) {
            SetTimerVisibility(true);

            Color barColor = UIStyle.GetColorForCountdownBar(recipe, timeRemaining);

            countdownBar.color = barColor;
            countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (timeRemaining / duration));
            countdownText.color = barColor;
            countdownText.text = timeRemaining.ToString("0.0") + "s";
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

            // We assume there's only one Slot
            if (ongoingSlots[0].Greedy)
                ongoingSlotImage.color = UIStyle.slotPink;
            else
                ongoingSlotImage.color = UIStyle.slotDefault;
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
                ongoingSlotArtImage.sprite = ResourcesManager.GetSpriteForElement(stack.Id);
                ongoingSlotArtImage.color = Color.white;
            }
        }

        public void DisplayComplete() {
            SetTimerVisibility(false);
            DisplayMiniSlot(null); 
        }

        public void SetCompletionCount(int newCount) {
            completionBadge.gameObject.SetActive(newCount > 0);
            completionText.text = newCount.ToString();
            /*
             * // Martin: Removed glow on completion count, it was muddying the feedback. Badge is enough, I think.
             * // I'd rather turn up the badge visibility
            if (newCount > 0) {
                SetGlowColor(UIStyle.TokenGlowColor.Default);
                ShowGlow(true);
            }
            else {
                ShowGlow(false);
            }
            */
        }

        #endregion

        #region -- Token interaction --------------------------

        // None of this should do view changes here. We're deferring to the SitController or TokenContainer

        public override void OnDrop(PointerEventData eventData) {
            if (DraggableToken.itemBeingDragged != null)
                DraggableToken.itemBeingDragged.InteractWithTokenDroppedOn(this);
        }

        public override void OnPointerClick(PointerEventData eventData) {
            if (!SituationController.IsOpen)
                OpenSituation();
            else
                CloseSituation();
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

        #endregion
    }    
}
