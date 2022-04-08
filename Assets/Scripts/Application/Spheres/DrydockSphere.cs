using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Manifestations;
using SecretHistories.Spheres;
using SecretHistories.States.TokenStates;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.UI
{

        [IsEmulousEncaustable(typeof(Sphere))]
        [RequireComponent(typeof(SphereDropCatcher))]
        [RequireComponent(typeof(TokenMovementReactionDecorator))]
        public class DrydockSphere : Sphere, IPointerEnterHandler, IPointerExitHandler, IInteractsWithTokens
        {

            public override SphereCategory SphereCategory => SphereCategory.Threshold;

            public override bool EmphasiseContents => true;
            public override bool DragOutToAnyRange => true;


        public GraphicFader slotGlow;
            protected virtual UIStyle.GlowTheme GlowTheme => UIStyle.GlowTheme.Classic;


            public override bool AllowStackMerge => true;


            public override bool AllowDrag => true;
            



            public virtual void Start()
            {
                slotGlow.Hide();
            }

            
            public virtual void OnPointerEnter(PointerEventData eventData)
            {
                if (GoverningSphereSpec.Greedy) // never show glow for greedy slots
                    return;

                if (CurrentlyBlockedFor(BlockDirection.Inward))
                    return;


            }

            public virtual void OnPointerExit(PointerEventData eventData)
            {
                if (GoverningSphereSpec.Greedy) // we're greedy? No interaction.
                    return;

                if (eventData.dragging)
                {
                    var pointerDrag = eventData.pointerDrag;

                    if (pointerDrag != null)
                    {
                        var potentialDragToken = eventData.pointerDrag.GetComponent<Token>();
                        if (potentialDragToken != null)
                            potentialDragToken.StopShowingPossibleInteractions();
                    }


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
            public override bool IsValidDestinationForToken(Token tokenToSend)
            {
                var alreadyIncoming = Watchman.Get<Xamanek>().GetCurrentItinerariesForPath(GetWildPath());
                if (alreadyIncoming.Any())
                    return false; //original flavour thresholds don't allow users or angels to put a token in if there are any en route already


                return base.IsValidDestinationForToken(tokenToSend);
            }



            public bool CanInteractWithToken(Token token)
            {

            if (CurrentlyBlockedFor(BlockDirection.Inward))
                    return false;

            return true;
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

