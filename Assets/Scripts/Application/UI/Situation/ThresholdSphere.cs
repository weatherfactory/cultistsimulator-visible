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
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;


namespace SecretHistories.UI
{

    [IsEmulousEncaustable(typeof(Sphere))]
    [RequireComponent(typeof(SphereDropCatcher))]
    [RequireComponent(typeof(TokenMovementReactionDecorator))]
    public class ThresholdSphere : Sphere, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,IInteractsWithTokens {

        public override SphereCategory SphereCategory => SphereCategory.Threshold;

        // VISUAL ELEMENTS
        public RecipeSlotViz viz;

        public TextMeshProUGUI SlotLabel;
        public Graphic border;
        public GraphicFader slotGlow;
        public LayoutGroup slotIconHolder;
        protected virtual UIStyle.GlowTheme GlowTheme=>UIStyle.GlowTheme.Classic;


        public override bool AllowStackMerge { get { return false; } }


        public override bool AllowDrag {
            get {
                return !GoverningSphereSpec.Greedy;
            }
        }



        public virtual void Start() {
            slotGlow.Hide();
        }


        public override void ApplySpec(SphereSpec sphereSpec)
        {
            base.ApplySpec(sphereSpec);

            if(SlotLabel!=null)
                SlotLabel.text = sphereSpec.Label;


            var angelsToAdd = sphereSpec.MakeAngels(this);
            foreach(var a in angelsToAdd)
                AddAngel(a);

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
            else
            {
                var draggedToken = eventData.pointerDrag.GetComponent<Token>();

                if (CanInteractWithToken(draggedToken)) {
                    draggedToken.ShowPossibleInteractionWithToken(draggedToken);

                    if (GetElementTokenInSlot() == null) // Only glow if the slot is empty
                        ShowHoverGlow();
                }
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
            if (GoverningSphereSpec.Greedy) // we're greedy? No interaction.
                return;

            if(eventData.dragging)
            {
                var potentialDragToken = eventData.pointerDrag.GetComponent<Token>();

                if ( potentialDragToken != null)
                    potentialDragToken.StopShowingPossibleInteractionWithToken(potentialDragToken);
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

        public override void DoRetirement(Action onRetirementComplete, SphereRetirementType retirementType)
        {
         //this should (and currently does, tho untidily) call Retire after it's done.
            HandleContentsGracefully(retirementType);

            if (gameObject.activeInHierarchy)
            {
                var fxCoroutine = viz.TriggerHideAnim(onRetirementComplete);
                StartCoroutine(fxCoroutine);
            }
            else
                onRetirementComplete();
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

            //does the token match the slot? Check that first
            ContainerMatchForStack match = GetMatchForTokenPayload(token.Payload);

            if (match.MatchType != SlotMatchForAspectsType.Okay)
            {
                token.CurrentState=new RejectedBySphereState();
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
                    NoonUtility.LogWarning("There's still a card in the slot when this reaches the slot; it wasn't intercepted by being dropped on the current occupant. Rework.");
                //currentOccupant.ReturnToTabletop();

                //now we put the token in the slot.
                token.CurrentState=new DroppedInSphereState();
               AcceptToken(token, context);
                SoundManager.PlaySfx("CardPutInSlot");
            }

            return true;
        }

        public override bool IsValidDestinationForToken(Token tokenToSend)
        {
            if (GetElementTokenInSlot() != null)
                return false;

            return base.IsValidDestinationForToken(tokenToSend);
        }

        public override TokenTravelItinerary GetItineraryFor(Token forToken)
        {

            Vector3 eventualTokenPosition;
            float destinationScale;
            if (_container.IsOpen)
            //the threshold is visible, so the reference position should be the sphere itself in world space
            {
                eventualTokenPosition = Choreographer.GetFreeLocalPosition(forToken, Vector2.zero);
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
                .WithDestinationSpherePath(GetAbsolutePath());


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
                .WithDestinationSpherePath(GetAbsolutePath());

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

        public void StopShowingPossibleInteractionWithToken(Token token)
        {
            SetGlowColor(UIStyle.GlowPurpose.Default);
            slotGlow.Hide(false);

        }

    }


    
}
