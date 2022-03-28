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
    public class ShelfSpaceSphere : Sphere, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IInteractsWithTokens
    {
        public override SphereCategory SphereCategory => SphereCategory.Threshold;

        // VISUAL ELEMENTS
        public GraphicFader slotGlow;
        public LayoutGroup slotIconHolder;
        protected virtual UIStyle.GlowTheme GlowTheme => UIStyle.GlowTheme.Classic;

        public override bool AllowStackMerge => false;
        
        public override bool AllowDrag => true;
        
        public override bool UnderstateContents => true;



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
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {


            if (!eventData.dragging)
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



        public override void DoRetirement(Action<SphereRetirementType> onRetirementComplete, SphereRetirementType retirementType)
        {
            //this should (and currently does, tho untidily) call Retire after it's done.
            HandleContentsGracefully(retirementType);

                onRetirementComplete(retirementType);
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

        public override void AcceptToken(Token token, Context context)
        {
            base.AcceptToken(token, context);

            
            SoundManager.PlaySfxOnceThisFrame("CardDrop");

            token.HideGhost();

        }


        public override bool TryAcceptToken(Token token, Context context)
        {
            if (!IsTokenInRangeOfThisShelf(token))
                return false;
        

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
           
            else
            {
                //it matches.
                //TODO: reject if full
                
                AcceptToken(token, context);
                
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

        public void StopShowingPossibleInteractions()
        {
            SetGlowColor(UIStyle.GlowPurpose.Default);
            slotGlow.Hide(false);

        }

        public override bool TryDisplayDropInteractionHere(Token forToken)
        {
            if (IsTokenInRangeOfThisShelf(forToken))
            {
                SetGlowColor(UIStyle.GlowPurpose.Default);
                return forToken.DisplayGhostAtChoreographerDrivenPosition(this);
            }
                
            return false;

        }
        public override bool DisplayGhostAt(Token forToken, Vector3 overridingWorldPosition)
        {
            return forToken.DisplayGhostAtChoreographerDrivenPosition(this, overridingWorldPosition);
        }


        private bool IsTokenInRangeOfThisShelf(Token token)
        {
            if (!token.CurrentState.ShouldObserveRangeLimits())
                return true;
            // TODO: refactor into more general Range logic in Sphere, along with similar code in Column
            //Get the home sphere location of this token.
            //Is it the same as this sphere?
            if (token.GetHomeSphere() == this)
                //if so, display ghost.
                return true;
            //Is this sphere's path contained in the home sphere's path
            var homeSpherePath = token.GetHomeSphere().GetAbsolutePath();
            if (homeSpherePath.Contains(this.GetAbsolutePath()))
                return true;

            //get the token path for this sphere and the token path for the home sphere - i.e. what tokens they live inside.
            //if they're the same, return true
            var homeSphereInToken = homeSpherePath.GetTokenPath();
            var thisInToken = GetAbsolutePath().GetTokenPath();
            if (homeSphereInToken == thisInToken)
                return true;

            //last possibility: the token paths for each might exist in the same sphere
            //e.g., a librarian and a shelf are in different tokens, but those are in the same room sphere.
            if (homeSphereInToken.GetSpherePath() == thisInToken.GetSpherePath())
                return true;

            //If neither of these, return false.
            return false;
        }
    }


    
}
