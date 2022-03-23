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
using SecretHistories.Constants;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;


namespace SecretHistories.UI //should be SecretHistories.Sphere. But that'll break save/load until I make save/load less fussy.
{

    [IsEmulousEncaustable(typeof(Sphere))]
    [RequireComponent(typeof(SphereDropCatcher))]
    [RequireComponent(typeof(TokenMovementReactionDecorator))]
    public class ThresholdSphere : Sphere, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,IInteractsWithTokens {

        public override SphereCategory SphereCategory => SphereCategory.Threshold;

        public override bool EmphasiseContents => true;

        // VISUAL ELEMENTS
        public RecipeSlotViz viz;

        public TextMeshProUGUI SlotLabel;
        public GraphicFader slotGlow;
        public LayoutGroup slotIconHolder;
        protected virtual UIStyle.GlowTheme GlowTheme=>UIStyle.GlowTheme.Classic;


        public override bool AllowStackMerge => false;


        public override bool AllowDrag {
            get {
                return !GoverningSphereSpec.Greedy;
            }
        }



        public virtual void Start() {
            slotGlow.Hide();
        }


        public override void SetPropertiesFromSpec(SphereSpec sphereSpec)
        {
            base.SetPropertiesFromSpec(sphereSpec);

            if(SlotLabel!=null)
                SlotLabel.text = sphereSpec.Label;

        }


        public virtual void OnPointerEnter(PointerEventData eventData) {
            if (GoverningSphereSpec.Greedy) // never show glow for greedy slots
                return;

            if (CurrentlyBlockedFor(BlockDirection.Inward))
                return;

            //if we're not dragging anything, and the slot is empty, glow the slot.
            if (!eventData.dragging) {
                if (GetElementTokenInSlot() == null)
                    ShowHoverGlow();
            }
           
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
            if (GoverningSphereSpec.Greedy) // we're greedy? No interaction.
                return;

            if (eventData.dragging)
            {
                var potentialDragToken = eventData.pointerDrag.GetComponent<Token>();

                if (potentialDragToken != null)
                    potentialDragToken.StopShowingPossibleInteractions();
            }

            HideHoverGlow();
        }

        public void SetGlowColor(UIStyle.GlowPurpose purposeType) {
            SetGlowColor(UIStyle.GetGlowColor(purposeType,GlowTheme));
        }

        public void SetGlowColor(Color color) {
            slotGlow.SetColor(color);
        }


        private  void ShowHoverGlow() {

                SetGlowColor(UIStyle.GlowPurpose.OnHover);
                SoundManager.PlaySfx("TokenHover");
                slotGlow.Show();

        }

        private void HideHoverGlow()
        {
            SetGlowColor(UIStyle.GlowPurpose.Default);
            slotGlow.Hide();
        }


        public override void DisplayAndPositionHere(Token token, Context context) {
            base.DisplayAndPositionHere(token, context);
            Choreographer.PlaceTokenAtFreeLocalPosition(token,context);

            if(slotIconHolder!=null)
                slotIconHolder.transform.SetAsLastSibling(); //this is p legacy hacky and exists just cos the hierarchy is how it is
            
        }

        public override void DoRetirement(Action<SphereRetirementType> onRetirementComplete, SphereRetirementType retirementType)
        {
         //this should (and currently does, tho untidily) call Retire after it's done.
            HandleContentsGracefully(retirementType);

            if (gameObject.activeInHierarchy)
            {
                var fxCoroutine = viz.TriggerHideAnim(onRetirementComplete);
                StartCoroutine(fxCoroutine);
            }
            else
                onRetirementComplete(retirementType);
        }
        public Token GetTokenInSlot()
        {
            List<Token> tokens = GetTokens().ToList();
            if(tokens.Count>1)
            {
                NoonUtility.Log("Something weird in slot " + GoverningSphereSpec.Id +
                                ": it has more than one token, so we're just returning the first.");
                return tokens.First();
            }

            return tokens.SingleOrDefault();
        }


        public Token GetElementTokenInSlot()
        {
            List<Token> elementTokens = GetElementTokens().ToList();
            if (elementTokens.Count > 1)
            {
                NoonUtility.Log("Something weird in slot " + GoverningSphereSpec.Id +
                                ": it has more than one stack, so we're just returning the first.");
                return elementTokens.First();

            }

            return elementTokens.SingleOrDefault();
        }


        public override void TryMoveAsideFor(Token potentialUsurper, Token incumbent, out bool incumbentMoved) {
            if (CurrentlyBlockedFor(BlockDirection.Inward)) { // We do not allow
                incumbentMoved = false;
                return;
            }

            //incomer is a token. Does it fit in the slot?
            if (GetMatchForTokenPayload(potentialUsurper.Payload).MatchType==SlotMatchForAspectsType.Okay && potentialUsurper.Quantity == 1)
            {
                incumbentMoved = true;
                incumbent.GoAway(new Context(Context.ActionSource.PlayerDrag)); //do this first; AcceptToken will trigger an update on the displayed aspects
                AcceptToken(potentialUsurper, new Context(Context.ActionSource.PlayerDrag));
            }
            else
                incumbentMoved = false;
        }

        public override bool TryAcceptToken(Token token,Context context)
        {
            if (Defunct)
                return false; //It would be cleaner to rely on the defunct check on TryAcceptToken() in 
            //the Sphere class. But I don't want to disentangle all the code below while in mid-bug-hunt.

            //does the token match the slot? Check that first
            ContainerMatchForStack match = GetMatchForTokenPayload(token.Payload);

            if (match.MatchType != SlotMatchForAspectsType.Okay)
            {
                token.CurrentState=new RejectedBySphereState();
                SoundManager.PlaySfx("CardDragFail");
                token.GoAway(context);

                if(match.MatchType==SlotMatchForAspectsType.ForbiddenAspectPresent || match.MatchType==SlotMatchForAspectsType.RequiredAspectMissing)
                {
                    UIDisplayMismatchInformation(match);
                }
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
                //it matches. Now we check if there's a token already there, and replace it if so:
                var currentOccupant = GetElementTokens().FirstOrDefault();

                // if we drop in the same slot where we came from, do nothing.
                if (currentOccupant == token)
                {
                    token.CurrentState=new DroppedInSphereState();
                    return false;
                }

                if (currentOccupant != null)
                {
                    //a token dropped on another token will usually already have driven it away via Token.OnDrop(). But it's possible that a token has snuck in here somehow in the meantime.
                    //When we come to do shelves, remember that there's also code in OnDrop to deal with overlap/usurpation.
                    currentOccupant.GoAway(new Context(Context.ActionSource.PlaceInThresholdUsurpedByIncomer));
                }
                //now we put the token in the slot.
                token.CurrentState=new DroppedInSphereState();
                AcceptToken(token, context);
              
                SoundManager.PlaySfx("CardPutInSlot");

                //When we add a token into a threshold sphere, we want to make sure its ghost is hidden.
                //This is very far from universally true on sphere entry, but it may be useful to promote this
                //to a basic sphere dichotomy like allowdrag.
                token.HideGhost();
            }

            return true;
        }

        private static void UIDisplayMismatchInformation(ContainerMatchForStack match)
        {
            var notifier = Watchman.Get<Notifier>();

            var compendium = Watchman.Get<Compendium>();

            if (notifier != null)
                notifier.ShowNotificationWindow(Watchman.Get<ILocStringProvider>().Get("UI_CANTPUT"),
                    match.GetProblemDescription(compendium), false);
        }

        public override bool IsValidDestinationForToken(Token tokenToSend)
        {
            if (GetElementTokenInSlot() != null)
                return false; //assumes only one

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


        public virtual TokenTravelItinerary GetItineraryFor(Token forToken,bool fum )
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

        public void OnPointerClick(PointerEventData eventData) {

            Watchman.Get<Notifier>().ShowSlotDetails(GoverningSphereSpec);

        }


        public bool CanInteractWithToken(Token token)
        {

            if (GetElementTokenInSlot() != null)
                return false; // Slot is filled? Don't highlight it as interactive
            if (CurrentlyBlockedFor(BlockDirection.Inward))
                return false; 


            if (!token.IsValidElementStack())
                return false; // we only accept stacks

            //does the token match the slot? Check that first
            ContainerMatchForStack match = GetMatchForTokenPayload(token.Payload);

            return match.MatchType == SlotMatchForAspectsType.Okay;
        }

        public void ShowPossibleInteractionWithToken(Token token)
        {
          SetGlowColor(UIStyle.GlowPurpose.Hint);
            slotGlow.Show(false);
        }

        public override bool TryDisplayDropInteractionHere(Token forToken)
        {


            if (CanInteractWithToken(forToken))
            {
                forToken.ShowReadyToInteract();
               // if (GetElementTokenInSlot() == null) // Only glow if the slot is empty
               //     ShowHoverGlow();
            }

            forToken.HideGhost();


            return true;
        }

        public void StopShowingPossibleInteractions()
        {
            SetGlowColor(UIStyle.GlowPurpose.Default);
            slotGlow.Hide(false);

        }

    }


    
}
