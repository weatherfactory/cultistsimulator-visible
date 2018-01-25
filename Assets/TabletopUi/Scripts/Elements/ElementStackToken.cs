﻿#pragma warning disable 0649
using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace Assets.CS.TabletopUI {
    public class ElementStackToken : DraggableToken, IElementStack, IGlowableView {

        [SerializeField] Image artwork;
        [SerializeField] Image backArtwork;
        [SerializeField] Image textBackground;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] ElementStackBadge stackBadge;
        [SerializeField] TextMeshProUGUI stackCountText;
        [SerializeField] GameObject decayView;
        [SerializeField] TextMeshProUGUI decayCountText;
        [SerializeField] Sprite spriteUniqueTextBG;

        [SerializeField] string defaultRetireFX = "CardBurn";

        private IElementStacksManager CurrentStacksManager;

        private Element _element;
        private int _quantity;

        private float lifetimeRemaining;
        private bool isFront = true;
        public Source StackSource { get; set; }

        private Coroutine turnCoroutine;
        private Coroutine animCoroutine;

        private ElementStackToken originStack = null; // if it was pulled from a stack, save that stack!

        public override string Id {
            get { return _element == null ? null : _element.Id; }
        }

        public bool Decays {
            get { return _element.Lifetime > 0; }
        }

        public int Quantity {
            get { return Defunct ? 0 : _quantity; }
        }

        public bool MarkedForConsumption { get; set; }

        #region -- Lifecycle  ------------------------------------------------------------------------------------

        protected override void Awake() {
            base.Awake();
        }

        protected void OnDisable() {
            // this resets any animation frames so we don't get stuck when deactivating mid-anim
            artwork.overrideSprite = null;

            // we're turning? Just set us to the garget
            if (turnCoroutine != null) {
                turnCoroutine = null;
                Flip(isFront, true); // instant to set where it wants to go
            }
        }

        #endregion 

        #region -- Set & Get Basic Values ------------------------------------------------------------------------------------

        public void SetQuantity(int quantity) {
            _quantity = quantity;
            if (quantity <= 0) {
                Retire(true);
                return;
            }
            DisplayInfo();
        }

        public void ModifyQuantity(int change) {
            SetQuantity(_quantity + change);
        }

        void SetAsUnique(bool unique) {
            if (unique)
                textBackground.overrideSprite = spriteUniqueTextBG;
            else
                textBackground.overrideSprite = null;
        }

        public Dictionary<string, string> GetXTriggers() {
            return _element.XTriggers;
        }

        public IAspectsDictionary GetAspects(bool includeSelf = true) {
            if (includeSelf)
                return _element.AspectsIncludingSelf;
            else
                return _element.Aspects;
        }

        public List<SlotSpecification> GetChildSlotSpecifications() {
            return _element.ChildSlotSpecifications;
        }

        public bool HasChildSlots() {
            return _element.HasChildSlots();
        }


        #endregion

        #region -- Create & populate ------------------------------------------------------------------------------------

        public void Populate(string elementId, int quantity, Source source) {
            _element = Registry.Retrieve<ICompendium>().GetElementById(elementId);

            try {
                SetQuantity(quantity); // this also toggles badge visibility through second call
                SetAsUnique(_element.Unique);

                name = "Card_" + elementId;
                if (_element == null)
                    NoonUtility.Log("Tried to populate token with unrecognised elementId:" + elementId);

                DisplayInfo();
                DisplayIcon();
                ShowGlow(false, false);
                ShowCardDecayTimer(false);
                SetCardDecay(0f);
                lifetimeRemaining = _element.Lifetime;

                StackSource = source;
                CurrentStacksManager = Registry.Retrieve<Limbo>().GetElementStacksManager(); //a stack must always have a parent stacks manager, or we get a null reference exception
                //when first created, it should be in Limbo
            }
            catch (Exception e) {
                NoonUtility.Log("Couldn't create element with ID " + elementId + " - " + e.Message);
                Retire(false);
            }
        }
        private void CullTextBackface() {
            decayCountText.enableCulling = true;
            stackCountText.enableCulling = true;
            text.enableCulling = true;
        }

        private void DisplayInfo() {
            text.text = _element.Label;
            stackBadge.gameObject.SetActive(Quantity > 1);
            stackCountText.text = Quantity.ToString();
        }

        private void DisplayIcon() {
            Sprite sprite = ResourcesManager.GetSpriteForElement(_element.Id);
            artwork.sprite = sprite;

            if (sprite == null)
                artwork.color = Color.clear;
            else
                artwork.color = Color.white;
        }

        #endregion

        #region -- Return To Tabletop ------------------------------------------------------------------------------------

        public override void ReturnToTabletop(INotification reason = null) {
            if (originStack != null && originStack.IsOnTabletop()) {
                originStack.MergeIntoStack(this);
                return;
            }
            // In case we're not unique and we've never been on the table, auto-merge us!
            else if (!_element.Unique && lastTablePos == null) {
                var tabletop = Registry.Retrieve<TabletopManager>();
                var stackManager = tabletop._tabletop.GetElementStacksManager();
                var existingStacks = stackManager.GetStacks();

                //check if there's an existing stack of that type to increment
                foreach (var stack in existingStacks) {
                    if (stack.Id == Id) {
                        var elementStack = stack as ElementStackToken;
                        elementStack.MergeIntoStack(this);
                        return;
                    }
                }
            }

            Registry.Retrieve<Choreographer>().ArrangeTokenOnTable(this);

            if (reason != null)
                notifier.TokenReturnedToTabletop(this, reason);
            /*
            if (lastTablePos != null)
                transform.position = (Vector3)lastTablePos;
            else
                lastTablePos = transform.position;

            DisplayAtTableLevel();
            */
        }

        private bool IsOnTabletop() {
            return transform.parent.GetComponent<TabletopTokenContainer>() != null;
        }

        private void MergeIntoStack(ElementStackToken merge) {
            SetQuantity(Quantity + merge.Quantity);
            merge.Retire(false);
        }

        #endregion

        #region -- Assign to Stack & Container ------------------------------------------------------------------------------------

        // Called from StacksManager
        public void SetStackManager(IElementStacksManager manager) {
            var oldStacksManager = CurrentStacksManager;
            CurrentStacksManager = manager;
            //notify afterwards, in case it counts the things *currently* in its list
            oldStacksManager.RemoveStack(this);
        }

        // Called from TokenContainer, usually after StacksManager told it to
        public override void SetTokenContainer(ITokenContainer newTokenContainer) {
            OldTokenContainer = TokenContainer;

            if (OldTokenContainer != null && OldTokenContainer != newTokenContainer)
                OldTokenContainer.SignalStackRemoved(this);

            TokenContainer = newTokenContainer;

            if (newTokenContainer != null)
                newTokenContainer.SignalStackAdded(this);
        }

        #endregion

        #region -- Retire + FX ------------------------------------------------------------------------------------

        public override bool Retire() {
            return Retire(defaultRetireFX);
        }

        public bool Retire(bool useDefaultFX) {
            return Retire(useDefaultFX ? defaultRetireFX : null);
        }

        public bool Retire(string vfxName) {
            if (Defunct)
                return false;

            SetStackManager(null); // Remove it from the StacksManager. It no longer exists in the model.
            SetTokenContainer(null); // notify the view container that we're no longer here

            //now take care of the Unity side of things.

            Defunct = true;
            AbortDrag(); // Make sure we have the drag aborted in case we're retiring mid-drag (merging stack frex)


            if (vfxName == "hide" || vfxName == "Hide") {
                StartCoroutine(FadeCard(0.5f));
            }
            else {
                // Check if we have an effect
                CardEffectRemove effect;

                if (string.IsNullOrEmpty(vfxName) || !gameObject.activeInHierarchy)
                    effect = null;
                else
                    effect = InstantiateEffect(vfxName);

                if (effect != null)
                    effect.StartAnim(this.transform);
                else
                    Destroy(gameObject);
            }

            return true;
        }

        CardEffectRemove InstantiateEffect(string effectName) {
            var prefab = Resources.Load("FX/RemoveCard/" + effectName);

            if (prefab == null)
                return null;

            var obj = Instantiate(prefab) as GameObject;

            if (obj == null)
                return null;

            return obj.GetComponent<CardEffectRemove>();
        }

        IEnumerator FadeCard(float fadeDuration) {
            float time = 0f;

            while (time < fadeDuration) {
                time += Time.deltaTime;
                canvasGroup.alpha = 1f - time / fadeDuration;
                yield return null;
            }

            Destroy(gameObject);
        }

        #endregion

        #region -- Allowed Interaction ------------------------------------------------------------------------------------

        protected override bool AllowsDrag() {
            return isFront && turnCoroutine == null; // no dragging while not front or busy turning
        }

        protected override bool AllowsInteraction() {
            // interaction is always possible on facedown cards to turn them back up
            return !isFront || base.AllowsInteraction();
        }

        public bool AllowsMerge() {
            if (Decays || _element.Unique)
                return false;
            else
                return TokenContainer.AllowStackMerge;
        }

        #endregion

        #region -- Interaction ------------------------------------------------------------------------------------

        public override void OnPointerClick(PointerEventData eventData) {
            if (isFront) {
                notifier.ShowElementDetails(_element);
            }
            else {
                FlipToFaceUp(false);
            }

            transform.SetAsLastSibling(); //this moves the clicked sibling on top of any other nearby cards.
        }

        public override void OnDrop(PointerEventData eventData) {
            if (DraggableToken.itemBeingDragged != null)
                DraggableToken.itemBeingDragged.InteractWithTokenDroppedOn(this);
        }

        public override bool CanInteractWithTokenDroppedOn(IElementStack stackDroppedOn) {
            return stackDroppedOn.Id == this.Id && stackDroppedOn.AllowsMerge();
        }

        public override void InteractWithTokenDroppedOn(IElementStack stackDroppedOn) {
            if (CanInteractWithTokenDroppedOn(stackDroppedOn)) {
                stackDroppedOn.SetQuantity(stackDroppedOn.Quantity + this.Quantity);
                DraggableToken.SetReturn(false, "was merged");
                SoundManager.PlaySfx("CardPutOnStack");

                var token = stackDroppedOn as DraggableToken;

                if (token != null) // make sure the glow is done in case we highlighted this
                    token.ShowGlow(false, true);

                this.Retire(false);
            }
            else {
                var droppedOnToken = stackDroppedOn as DraggableToken;
                bool moveAsideFor = false;
                droppedOnToken.TokenContainer.TryMoveAsideFor(this, droppedOnToken, out moveAsideFor);

                if (moveAsideFor)
                    DraggableToken.SetReturn(false, "was moved aside for");
            }
        }

        public void SplitAllButNCardsToNewStack(int n) {
            if (Quantity > n) {
                var cardLeftBehind = PrefabFactory.CreateToken<ElementStackToken>(transform.parent);
                cardLeftBehind.Populate(Id, Quantity - n, Source.Existing());

                originStack = cardLeftBehind;

                //goes weird when we pick things up from a slot. Do we need to refactor to Accept/Gateway in order to fix?
                SetQuantity(1);

                // Accepting stack will trigger overlap checks, so make sure we're not in the default pos but where we want to be.
                cardLeftBehind.transform.position = transform.position;

                var stacksManager = TokenContainer.GetElementStacksManager();
                stacksManager.AcceptStack(cardLeftBehind);

                // Accepting stack may put it to pos Vector3.zero, so this is last
                cardLeftBehind.transform.position = transform.position;
            }
        }

        protected override void StartDrag(PointerEventData eventData) {
            // to ensure these are set before we split the cards
            DraggableToken.itemBeingDragged = this; 
            IsInAir = true;

            // A bit hacky, but it works: DID NOT start dragging from badge? Split cards 
            if (stackBadge.IsHovering() == false)
                SplitAllButNCardsToNewStack(1);

            base.StartDrag(eventData); // To ensure all events fire at the end
        }

        public override bool CanInteractWithTokenDroppedOn(SituationToken tokenDroppedOn) {
            return tokenDroppedOn.SituationController.CanTakeDroppedToken(this);
        }

        public override void InteractWithTokenDroppedOn(SituationToken tokenDroppedOn) {
            if (CanInteractWithTokenDroppedOn(tokenDroppedOn)) {
                if (!tokenDroppedOn.SituationController.IsOpen)
                    tokenDroppedOn.OpenSituation();
                else
                    tokenDroppedOn.OpenToken(); // this turns off the glow

                tokenDroppedOn.SituationController.PushDraggedStackIntoStartingSlots(this);
                return;
            }

            bool moveAsideFor = false;
            tokenDroppedOn.TokenContainer.TryMoveAsideFor(this, tokenDroppedOn, out moveAsideFor);

            if (moveAsideFor)
                DraggableToken.SetReturn(false, "was moved aside for");
            else
                DraggableToken.SetReturn(true);
        }

        #endregion

        #region -- Decay & Timers ------------------------------------------------------------------------------------

        public void Decay(float interval) {
            if (!Decays)
                return;

            lifetimeRemaining = lifetimeRemaining - interval;

            if (lifetimeRemaining < 0)
                Retire(true);

            if (lifetimeRemaining < _element.Lifetime / 2) {
                ShowCardDecayTimer(true);
                SetCardDecayTime(lifetimeRemaining);
            }

            SetCardDecay(1 - lifetimeRemaining / _element.Lifetime);

        }
        // Card Decay Timer
        public void ShowCardDecayTimer(bool showTimer) {
            decayView.gameObject.SetActive(showTimer);
        }

        public void SetCardDecayTime(float timeRemaining) {

            decayCountText.text = timeRemaining.ToString("0.0") + "s";
        }

        public void SetCardDecay(float percentage) {
            percentage = Mathf.Clamp01(percentage);
            artwork.color = new Color(1f - percentage, 1f - percentage, 1f - percentage, 1.5f - percentage);
        }

        #endregion

        #region -- Turn Card ------------------------------------------------------------------------------------

        public void FlipToFaceUp(bool instant = false) {
            Flip(true, instant);
        }

        public void FlipToFaceDown(bool instant = false) {
            Flip(false, instant);
        }

        public void Flip(bool state, bool instant = false) {
            if (isFront == state && !instant) // if we're instant, ignore this to allow forcing of pos
                return;

            ShowGlow(!state); // disable face-down hover-effect

            isFront = state;
            //if a card has just been turned face up in a situation, it's now an existing, established card
            if (isFront && StackSource.SourceType == SourceType.Fresh)
                StackSource.SourceType = SourceType.Existing;

            if (gameObject.activeInHierarchy == false || instant) {
                transform.localRotation = GetFrontRotation(isFront);
                return;
            }

            if (turnCoroutine != null)
                StopCoroutine(turnCoroutine);

            turnCoroutine = StartCoroutine(DoTurn());
        }

        Quaternion GetFrontRotation(bool isFront) {
            return Quaternion.Euler(0f, isFront ? 0f : 180f, 0f);
        }

        public bool IsFront() {
            return isFront;
        }

        IEnumerator DoTurn() {
            float time = 0f;
            float targetAngle = isFront ? 0f : 180f;
            float currentAngle = transform.localEulerAngles.y;
            float duration = Mathf.Abs(targetAngle - currentAngle) / 900f;

            while (time < duration) {
                time += Time.deltaTime;
                transform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(currentAngle, targetAngle, time / duration), 0f);
                yield return null;
            }

            transform.localRotation = Quaternion.Euler(0f, targetAngle, 0f);
            turnCoroutine = null;
        }

        public void SetBackface(string backId) {
            Sprite sprite;

            if (string.IsNullOrEmpty(backId))
                sprite = null;
            else
                sprite = ResourcesManager.GetSpriteForCardBack(backId);

            backArtwork.overrideSprite = sprite;
        }

        #endregion

        #region -- Animated Card ------------------------------------------------------------------------------------

        public bool CanAnimate() {
            if (gameObject.activeInHierarchy == false)
                return false; // can not animate if deactivated

            return _element.AnimFrames > 0;
        }

        /// <summary>
        /// Trigger an animation on the card
        /// </summary>
        /// <param name="duration">Determines how long the animation runs. Time is spent equally on all frames</param>
        /// <param name="frameCount">How many frames to show. Default is 1</param>
        /// <param name="frameIndex">At which frame to start. Default is 0</param>
        public void StartArtAnimation() {
            if (!CanAnimate())
                return;

            if (animCoroutine != null)
                StopCoroutine(animCoroutine);

            // TODO: pull data from element itself and use that to drive the values below
            float duration = 0.2f;
            int frameCount = _element.AnimFrames;
            int frameIndex = 0;

            animCoroutine = StartCoroutine(DoAnim(duration, frameCount, frameIndex));
        }

        IEnumerator DoAnim(float duration, int frameCount, int frameIndex) {
            Sprite[] animSprites = new Sprite[frameCount];

            for (int i = 0; i < animSprites.Length; i++)
                animSprites[i] = ResourcesManager.GetSpriteForElement(Id, frameIndex + i);

            float time = 0f;
            int spriteIndex = -1;
            int lastSpriteIndex = -1;

            while (time < duration) {
                time += Time.deltaTime;
                spriteIndex = (frameCount == 1 ? 0 : Mathf.FloorToInt(time / duration * frameCount));

                if (spriteIndex != lastSpriteIndex) {
                    lastSpriteIndex = spriteIndex;
                    // Ternary operator since the spriteIndex math will sometimes result in the last frame popping out of range, which is fine.
                    artwork.overrideSprite = (spriteIndex < animSprites.Length ? animSprites[spriteIndex] : null);
                }
                yield return null;
            }

            // remove anim 
            artwork.overrideSprite = null;
        }

        #endregion

    }
}
