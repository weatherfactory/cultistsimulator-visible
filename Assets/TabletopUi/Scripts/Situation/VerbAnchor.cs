using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            _manifestation  = Registry.Get<PrefabFactory>().CreateManifestationPrefab(situation.Species.AnchorManifestationType, this.transform);
            

            if (Verb.Transient)
                _durability = AnchorDurability.Transient;
            else
                _durability = AnchorDurability.Enduring;

            name = "Verb_" + EntityId;


            _manifestation.InitialiseVisuals(Verb);
            _manifestation.DisplaySpheres(new List<Sphere>());
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
            
            return (_situation.GetFirstAvailableThresholdForStackPush(stackDroppedOn).SphereCategory==SphereCategory.Threshold);
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


        public void DisplaySituationState(Situation situation)
        {
            switch (situation.State)
            {
                case SituationState.Unstarted:
                    _manifestation.SendNotification(new Notification(string.Empty,"-1"));
                    _manifestation.DisplaySpheres(new List<Sphere>());
                    break;
                case SituationState.Ongoing:
                    _manifestation.SendNotification(new Notification(string.Empty, "-1"));
                    if (situation.CurrentBeginningEffectCommand!=null)
                    {
                        //if (situation.CurrentBeginningEffectCommand.OngoingSlots.Any())
                        //    _manifestation.DisplaySpheres(situation.CurrentBeginningEffectCommand.OngoingSlots[0].Greedy);
                        if(!string.IsNullOrEmpty(situation.CurrentBeginningEffectCommand.BurnImage))
                            BurnImageUnderToken(situation.CurrentBeginningEffectCommand.BurnImage);
                    }

                    _manifestation.UpdateTimerVisuals(situation.Warmup, situation.TimeRemaining,situation.intervalForLastHeartbeat,false,situation.currentPrimaryRecipe.SignalEndingFlavour);
                    break;

                case SituationState.Complete:
                    _manifestation.UpdateTimerVisuals(situation.Warmup, situation.TimeRemaining, situation.intervalForLastHeartbeat, false, situation.currentPrimaryRecipe.SignalEndingFlavour);

                    int completionCount = situation.GetStacks(SphereCategory.Output).Select(s => s.Quantity).Sum();

                    _manifestation.SendNotification(new Notification(string.Empty, completionCount.ToString()));
                    break;
            }
        }


        public void ContainerContentsUpdated(Situation situation)
        {

            var thresholdSpheresWithStacks = situation.GetSpheresByCategory(SphereCategory.Threshold)
                .Where(sphere => sphere.GetStacks().Count() == 1);
            
            _manifestation.DisplaySpheres(thresholdSpheresWithStacks);
            

            int completionCount =situation.GetStacks(SphereCategory.Output).Select(s => s.Quantity).Sum();
            _manifestation.SendNotification(new Notification(string.Empty, completionCount.ToString()));

        }

        public void ReceiveNotification(INotification n)
        {
            _manifestation.SendNotification(n);
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

        private void SwapOutManifestation(IManifestation oldManifestation, IManifestation newManifestation, RetirementVFX vfxForOldManifestation)
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
