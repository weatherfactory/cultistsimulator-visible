using System;
using System.Collections.Generic;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using SecretHistories.States.TokenStates;
using System.Linq;
using SecretHistories.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace SecretHistories.UI
{

    [IsEmulousEncaustable(typeof(Sphere))]
    [RequireComponent(typeof(SphereDropCatcher))]
    [RequireComponent(typeof(TokenMovementReactionDecorator))]
    public class ShelfSpaceSphere : Sphere, IInteractsWithTokens
    {
        public override SphereCategory SphereCategory => SphereCategory.Threshold;

        // VISUAL ELEMENTS
        public GraphicFader slotGlow;
        public LayoutGroup slotIconHolder;
        protected virtual UIStyle.GlowTheme GlowTheme => UIStyle.GlowTheme.Classic;

        public override bool AllowStackMerge => false;


        public override bool AllowDrag => true;


        public virtual void Start()
        {
            slotGlow.Hide();
        }


        
        public virtual void OnPointerEnter(PointerEventData eventData)
        {

            if (CurrentlyBlockedFor(BlockDirection.Inward))
                return;

            //if we're not dragging anything, and the slot is empty, glow the slot.
            if (!eventData.dragging)
            {
 ShowHoverGlow();
            }
            else
            {
                var draggedToken = eventData.pointerDrag.GetComponent<Token>();

                if (CanInteractWithToken(draggedToken))
                {
                    draggedToken.ShowPossibleInteractionWithToken(draggedToken);

                        ShowHoverGlow();
                }
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {


            if (eventData.dragging)
            {
                var potentialDragToken = eventData.pointerDrag.GetComponent<Token>();

                if (potentialDragToken != null)
                    potentialDragToken.StopShowingPossibleInteractionWithToken(potentialDragToken);
            }

            HideHoverGlow();
        }

        public void SetGlowColor(UIStyle.GlowPurpose purposeType)
        {
            SetGlowColor(UIStyle.GetGlowColor(purposeType, GlowTheme));
        }

        public void SetGlowColor(Color color)
        {
            slotGlow.SetColor(color);
        }


        private void ShowHoverGlow()
        {

            SetGlowColor(UIStyle.GlowPurpose.OnHover);
            SoundManager.PlaySfx("TokenHover");
            slotGlow.Show();

        }

        private void HideHoverGlow()
        {
            SetGlowColor(UIStyle.GlowPurpose.Default);
            slotGlow.Hide();
        }


        public override void DisplayAndPositionHere(Token token, Context context)
        {
            base.DisplayAndPositionHere(token, context);
            Choreographer.PlaceTokenAtFreeLocalPosition(token, context);

            if (slotIconHolder != null)
                slotIconHolder.transform.SetAsLastSibling(); //this is p legacy hacky and exists just cos the hierarchy is how it is

        }

        public override void DoRetirement(Action onRetirementComplete, SphereRetirementType retirementType)
        {
            //this should (and currently does, tho untidily) call Retire after it's done.
            HandleContentsGracefully(retirementType);

                onRetirementComplete();
        }




        public override void TryMoveAsideFor(Token potentialUsurper, Token incumbent, out bool incumbentMoved)
        {
            if (CurrentlyBlockedFor(BlockDirection.Inward))
            { // We do not allow
                incumbentMoved = false;
                return;
            }

            //incomer is a token. Does it fit in the slot?
            if (GetMatchForTokenPayload(potentialUsurper.Payload).MatchType == SlotMatchForAspectsType.Okay && potentialUsurper.Quantity == 1)
            {
                incumbentMoved = true;
                incumbent.GoAway(new Context(Context.ActionSource.PlayerDrag)); //do this first; AcceptToken will trigger an update on the displayed aspects
                AcceptToken(potentialUsurper, new Context(Context.ActionSource.PlayerDrag));
            }
            else
                incumbentMoved = false;
        }

        public override bool TryAcceptToken(Token token, Context context)
        {

            //does the token match the slot? Check that first
            ContainerMatchForStack match = GetMatchForTokenPayload(token.Payload);

            if (match.MatchType != SlotMatchForAspectsType.Okay)
            {
                token.CurrentState = new RejectedBySphereState();
                SoundManager.PlaySfx("CardDragFail");
                token.GoAway(context);

                var notifier = Watchman.Get<Notifier>();

                var compendium = Watchman.Get<Compendium>();

                if (notifier != null)
                    notifier.ShowNotificationWindow(Watchman.Get<ILocStringProvider>().Get("UI_CANTPUT"), match.GetProblemDescription(compendium), false);
            }
            else if (token.Quantity != 1)
            {
                // We're dropping a stack of >1?
                // set main stack to be returned to start position
                token.CurrentState = new RejectedViaSplit();
                // And we split a new one that's 1 (leaving the returning card to be n-1)
                var newStack = token.CalveToken(1, new Context(Context.ActionSource.PlayerDrag));
                // And we put that into the slot
                AcceptToken(newStack, context);
                return false; //We've accepted the *new* calved token, but we don't return true because the remaining, original token is rejected.
            }
            else
            {
                //it matches.
                //TODO: reject if full

                //now we put the token in the slot.
                token.CurrentState = new DroppedInSphereState();
                AcceptToken(token, context);
                SoundManager.PlaySfx("CardPutInSlot");
            }

            return true;
        }

        public override bool IsValidDestinationForToken(Token tokenToSend)
        {
            //TODO: reject if full

            var alreadyIncoming = Watchman.Get<Xamanek>().GetCurrentItinerariesForPath(GetWildPath());
            if (alreadyIncoming.Any())
                return false; //original flavour thresholds don't allow users or angels to put a token in if there are any en route already


            return base.IsValidDestinationForToken(tokenToSend);
        }

        public override TokenTravelItinerary GetItineraryFor(Token forToken)
        {

            Vector3 eventualTokenPosition;
            float destinationScale;
            if (_container.IsOpen)
            //the threshold is visible, so the reference position should be the sphere itself in world space
            {
                eventualTokenPosition = Choreographer.GetClosestFreeLocalPosition(forToken, Vector2.zero);
                destinationScale = 1f;
            }
            else
            {
                var worldPosOfToken = _container.GetRectTransform().position;
                eventualTokenPosition = this.GetRectTransform().InverseTransformPoint(worldPosOfToken);
                destinationScale = 0.35f;
            }

            TokenTravelItinerary itinerary = new TokenTravelItinerary(forToken.Location.Anchored3DPosition,
                    eventualTokenPosition)
                .WithScaling(1f, destinationScale)
                .WithDestinationSpherePath(GetWildPath());


            return itinerary;

        }


        public virtual TokenTravelItinerary GetItineraryFor(Token forToken, bool fum)
        {
            var hereAsWorldPosition = GetRectTransform().position;

            var currentSphere = Watchman.Get<HornedAxe>().GetSphereByPath(forToken.Location.AtSpherePath);
            var otherSphereTransform = currentSphere.GetRectTransform();
            var bestGuessReferencePosition = otherSphereTransform.InverseTransformPoint(hereAsWorldPosition);

            TokenTravelItinerary itinerary = new TokenTravelItinerary(forToken.Location.Anchored3DPosition,
                    bestGuessReferencePosition)
                .WithScaling(1f, 0.35f)
                .WithDestinationSpherePath(GetWildPath());

            return itinerary;
        }

        public void OnPointerClick(PointerEventData eventData)
        {

            Watchman.Get<Notifier>().ShowSlotDetails(GoverningSphereSpec);

        }


        public bool CanInteractWithToken(Token token)
        {

            if (CurrentlyBlockedFor(BlockDirection.Inward))
                return false;

            
            //does the token match the slot? Check that first
            ContainerMatchForStack match = GetMatchForTokenPayload(token.Payload);

            return match.MatchType == SlotMatchForAspectsType.Okay;
        }

        public void ShowPossibleInteractionWithToken(Token token)
        {
            SetGlowColor(UIStyle.GlowPurpose.Hint);
            slotGlow.Show(false);
        }

        public void StopShowingPossibleInteractionWithToken(Token token)
        {
            SetGlowColor(UIStyle.GlowPurpose.Default);
            slotGlow.Hide(false);

        }

        public override bool TryDisplayGhost(Token forToken)
        {
            return forToken.DisplayGhostAtChoreographerDrivenPosition(this);

        }
        public override bool TryDisplayGhost(Token forToken, Vector3 overridingWorldPosition)
        {
            return forToken.DisplayGhostAtChoreographerDrivenPosition(this, overridingWorldPosition);
        }
    }


    
}
