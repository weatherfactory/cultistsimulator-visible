using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
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

        private Situation _situation;

        public IVerb Verb
        {
            get { return _situation.Verb; }
        }
        private IAnchorManifestation _manifestation;

        private AnchorDurability _durability;


        public AnchorDurability Durability
        {
            get { return _durability; }
        }



        public override string EntityId
        {
            get { return Verb.Id; }
        }


        public void Populate(Situation situation)
        {
            _situation = situation;
            _manifestation  = Registry.Get<PrefabFactory>().CreateAnchorManifestationPrefab(situation.Species.AnchorManifestationType, this.transform);
            

            if (Verb.Transient)
                _durability = AnchorDurability.Transient;
            else
                _durability = AnchorDurability.Enduring;

            name = "Verb_" + EntityId;


            _manifestation.InitialiseVisuals(Verb);
            _manifestation.DisplayStackInMiniSlot(null);
            SetParticleSimulationSpace(transform.parent);

        }


        public override void ReturnToTabletop(Context context) {
            Registry.Get<Choreographer>().ArrangeTokenOnTable(this, context);
        }


        public void DisplayOverrideIcon(string icon)
        {
            _manifestation.OverrideIcon(icon);
        }

        public void SetParticleSimulationSpace(Transform transform)
        {
          _manifestation.SetParticleSimulationSpace(transform);
        }


        public override void ReactToDraggedToken(TokenInteractionEventArgs args)
        {
            if (args.TokenInteractionType == TokenInteractionType.BeginDrag)
            {

                var stack = args.Token as ElementStackToken;
                if (stack == null)
                    return;
                if (!_situation.GetFirstAvailableThresholdForStackPush(stack).CurrentlyBlockedFor(BlockDirection.Inward))
                {
                    _manifestation.Highlight(HighlightType.CanInteractWithOtherToken);
                }
            }
            if (args.TokenInteractionType == TokenInteractionType.EndDrag)
               _manifestation.Unhighlight(HighlightType.CanInteractWithOtherToken);

        }

        public override void HighlightPotentialInteractionWithToken(bool show)
        {
            _manifestation.Highlight(HighlightType.CanInteractWithOtherToken);


        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if(!eventData.dragging) 
                _manifestation.Highlight(HighlightType.Hover);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!eventData.dragging)
                _manifestation.Unhighlight(HighlightType.Hover);

        }


        
        public void DisplayAsOpen() {
            _manifestation.Unhighlight(HighlightType.All);
        }

        public void DisplayAsClosed() {
            _manifestation.Unhighlight(HighlightType.All);
        }



        public Vector3 GetOngoingSlotPosition()
        {
            return rectTransform.anchoredPosition3D + _manifestation.GetOngoingSlotPosition();
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

            InteractWithIncomingObject(eventData.pointerDrag);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {

            if (!_manifestation.HandleClick(eventData, this))
            {
                //Manifestation didn't handle click
                Registry.Get<DebugTools>().SetInput(_situation.RecipeId);

                if (!_situation.IsOpen)
                    OpenSituation();
                else
                    CloseSituation();
            }

        }



        public void OpenSituation() {
            _situation.OpenAtCurrentLocation();
        }

        void CloseSituation() {
            _situation.Close();
        }

        public override bool CanInteractWithIncomingObject(ElementStackToken stackDroppedOn) {
            
            return (_situation.GetFirstAvailableThresholdForStackPush(stackDroppedOn).ContainerCategory==ContainerCategory.Threshold);
        }

        public override void InteractWithIncomingObject(ElementStackToken incomingStack)
        {
            
            if (CanInteractWithIncomingObject(incomingStack))
            {
                // This will put it into the ongoing or the starting slot, token determines
                _situation.PushDraggedStackIntoThreshold(incomingStack);

                // Then we open the situation (cause this closes other situations and this may return the stack we try to move
                // back onto the tabletop - if it was in its starting slots. - Martin
                if (!_situation.IsOpen)
                    OpenSituation();
                else
                    DisplayAsOpen();
                return;
            }

            // We can't interact? Then dump us on the tabletop
            SetXNess(TokenXNess.ElementDroppedOnTokenButCannotInteractWithIt);
            ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));

            
        }

        public override bool CanInteractWithIncomingObject(VerbAnchor tokenDroppedOn) {
            //verb dropped on verb - OK
            return false; // a verb anchor can't be dropped on anything
        }

        public override void InteractWithIncomingObject(VerbAnchor tokenDroppedOn) {
            //verb dropped on verb - OK
            
            tokenDroppedOn.Sphere.TryMoveAsideFor(this, tokenDroppedOn, out bool moveAsideFor);

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
            if (!CanAnimateArt())
                return;
            _manifestation.BeginArtAnimation();

        }

      

        public override bool CanAnimateArt()
        {
            if (gameObject == null)
                return false;

            if (gameObject.activeInHierarchy == false)
                return false; // can not animate if deactivated

            return _manifestation.CanAnimate();
        }


        public void DisplaySituationState(SituationEventData e)
        {
            switch (e.SituationState)
            {
                case SituationState.Unstarted:
                    _manifestation.SetCompletionCount(-1);
                    _manifestation.DisplayStackInMiniSlot(null);
                    break;
                case SituationState.Ongoing:
                    _manifestation.SetCompletionCount(-1);
                    if (e.BeginningEffectCommand!=null)
                    {
                        if (e.BeginningEffectCommand.OngoingSlots.Any())
                            _manifestation.ShowMiniSlot(e.BeginningEffectCommand.OngoingSlots[0].Greedy);
                        if(!string.IsNullOrEmpty(e.BeginningEffectCommand.BurnImage))
                            BurnImageUnderToken(e.BeginningEffectCommand.BurnImage);
                    }

                    _manifestation.UpdateTimerVisuals(e.Warmup, e.TimeRemaining, e.CurrentRecipe.SignalEndingFlavour);
                    break;

                case SituationState.Complete:
                    _manifestation.DisplayComplete();

                    int completionCount = e.StacksInEachStorage[ContainerCategory.Output].Select(s => s.Quantity).Sum();

                    _manifestation.SetCompletionCount(completionCount);
                    break;
            }
        }


        public void ContainerContentsUpdated(SituationEventData e)
        {
            
            var ongoingStacks = e.StacksInEachStorage[ContainerCategory.Threshold];
            if(ongoingStacks.Count == 1)
                _manifestation.DisplayStackInMiniSlot(ongoingStacks.First());
            else
              _manifestation.DisplayStackInMiniSlot(null);
            
            _manifestation.SetCompletionCount(e.StacksInEachStorage[ContainerCategory.Output].Count);

        }

        public void ReceiveNotification(SituationEventData e)
        {
            _manifestation.ReceiveAndRefineTextNotification(e.Notification);
        }

        public void RecipePredicted(RecipePrediction recipePrediction)
        {
            
        }

       public override void AnimateTo(float duration, Vector3 startPos, Vector3 endPos, Action<AbstractToken> animDone, float startScale = 1f, float endScale = 1f)
        {
            _manifestation.AnimateTo(this,duration,startPos,endPos,animDone,startScale,endScale);
        }

        private void animDone(VerbAnchor token)
        {
            Sphere.DisplayHere(token, new Context(Context.ActionSource.AnimEnd));
        }

        public void DumpOutputStacks()
        {
            _situation.CollectOutputStacks();
        }

        public override void MoveObject(PointerEventData eventData) {
            Vector3 dragPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(Registry.Get<IDraggableHolder>().RectTransform, eventData.position, eventData.pressEventCamera, out dragPos);

            _manifestation.DoMove(rectTransform);

            // Potentially change this so it is using UI coords and the RectTransform?
            rectTransform.position = new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y, dragPos.z + dragHeight);

            // rotate object slightly based on pointer Delta
            if (rotateOnDrag && eventData.delta.sqrMagnitude > 10f) {
                // This needs some tweaking so that it feels more responsive, physica. Card rotates into the direction you swing it?
                perlinRotationPoint += eventData.delta.sqrMagnitude * 0.001f;
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -10 + Mathf.PerlinNoise(perlinRotationPoint, 0) * 20));
            }
        }

        private void SwapOutManifestation(IAnchorManifestation oldManifestation, IAnchorManifestation newManifestation, RetirementVFX vfxForOldManifestation)
        {
            var manifestationToRetire = oldManifestation;
            _manifestation = newManifestation;

            manifestationToRetire.Retire(vfxForOldManifestation, OnSwappedOutManifestationRetired);

        }

        private void OnSwappedOutManifestationRetired()
        {

        }

   
    }
}
