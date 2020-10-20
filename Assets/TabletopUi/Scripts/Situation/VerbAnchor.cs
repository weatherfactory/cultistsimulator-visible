using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Logic;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
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


        private IVerb _verb;
        private IAnchorManifestation _manifestation;

        private AnchorDurability _durability;

        public SituationController SituationController { get; private set; }

        public AnchorDurability Durability
        {
            get { return _durability; }
        }



        public override string EntityId
        {
            get { return _verb.Id; }
        }


        public void Initialise(IVerb verb, SituationController sc)
        {

            _manifestation = TokenContainer.CreateAnchorManifestation(this);

            if (verb.Transient)
                _durability = AnchorDurability.Transient;
            else
                _durability = AnchorDurability.Enduring;

            SituationController = sc;
            name = "Verb_" + EntityId;
    _manifestation.DisplayStackInMiniSlot(null);            

        }


        public override void ReturnToTabletop(Context context) {
            Registry.Get<Choreographer>().ArrangeTokenOnTable(this, context);
        }


        public void DisplayOverrideIcon(string icon)
        {
            displayIcon(icon);
        }

        public void SetParticleSimulationSpace(Transform transform)
        {
            throw new NotImplementedException();
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
                    _manifestation.Highlight(HighlightType.CanInteractWithOtherToken);
                }
            }
            if (args.TokenInteractionType == TokenInteractionType.EndDrag)
               _manifestation.Unhighlight(HighlightType.CanInteractWithOtherToken);

        }

        public override void HighlightPotentialInteractionWithToken(bool show)
        {
            ShowHoverGlow(show, false, UIStyle.brightPink);

        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            _manifestation.Highlight(HighlightType.Hover);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            _manifestation.Unhighlight(HighlightType.Hover);

        }


        
        public void DisplayAsOpen() {
            _manifestation.Unhighlight(HighlightType.All);
        }

        public void DisplayAsClosed() {
            _manifestation.Unhighlight(HighlightType.All);
        }



        public Vector3 GetOngoingSlotPosition() {
            return rectTransform.anchoredPosition3D + ongoingSlotImage.rectTransform.anchoredPosition3D;
        }



        public void DisplayTimeRemaining(float duration, float timeRemaining, EndingFlavour signalEndingFlavour)
        {
            throw new NotImplementedException();
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
            SetXNess(TokenXNess.ElementDroppedOnTokenButCannotInteractWithIt);
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
                SetXNess(TokenXNess.DroppedOnTokenWhichMovedAside);
            else
                SetXNess(TokenXNess.DroppedOnTokenWhichWontMoveAside);

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
            _manifestation.BeginArtAnimation();

        }

      

        public override bool CanAnimate()
        {
            if (gameObject == null)
                return false;

            if (gameObject.activeInHierarchy == false)
                return false; // can not animate if deactivated

            return _manifestation.CanAnimate();
        }


        public void SituationBeginning(SituationEvent e)
        {
          _manifestation.DisplayStackInMiniSlot(null);
          if(e.CurrentRecipe.Slots.Count==1)
              _manifestation.ShowMiniSlot(e.CurrentRecipe.Slots[0].Greedy);
            
        }

        public void SituationOngoing(SituationEvent e)
        {
        _manifestation.UpdateTimerVisuals(e.Warmup, e.TimeRemaining, e.CurrentRecipe.SignalEndingFlavour);
        }

        public void SituationExecutingRecipe(SituationEvent e)
        {
          //
        }

        public void SituationComplete(SituationEvent e)
        {
     _manifestation.DisplayComplete();
     _manifestation.SetCompletionCount(e.StacksInEachStorage[ContainerRelation.Output].Count);
     _manifestation.DisplayStackInMiniSlot(e.StacksInEachStorage[ContainerRelation.Ongoing].FirstOrDefault());

        }

        public void ResetSituation()
        {
            _manifestation.SetCompletionCount(-1);
    }

        public void ReceiveAndRefineTextNotification(SituationEvent e)
        {
            _manifestation.ReceiveAndRefineTextNotification(e.notification);
        }


        }



    public class SituationEvent
    {
        public float Warmup { get; set; }
        public float TimeRemaining { get; set; }
        public Recipe CurrentRecipe; //replace with SituationCreationComand / SituationEffectCommand? combine CreationCommand and EffectCommand?
        public Dictionary<ContainerRelation, List<ElementStackToken>> StacksInEachStorage { get; set; }
        public SituationEffectCommand effect;
        public INotification notification;

        public SituationEvent()
        {
         StacksInEachStorage=new Dictionary<ContainerRelation, List<ElementStackToken>>();
         CurrentRecipe = NullRecipe.Create();
         effect = new SituationEffectCommand();
        }
    }
}
