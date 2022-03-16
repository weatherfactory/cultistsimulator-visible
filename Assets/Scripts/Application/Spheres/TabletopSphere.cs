using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Entities.Verbs;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Spheres.Angels;
using SecretHistories.States.TokenStates;
using SecretHistories.UI.Scripts;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Services;
using SecretHistories.Spheres;

using UnityEngine.EventSystems;

namespace SecretHistories.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]
    public class TabletopSphere : Sphere
    {
#pragma warning disable 649
        [SerializeField] TabletopBackground _background;
        [SerializeField] protected CanvasGroupFader canvasGroupFader;
#pragma warning disable 649

        public override SphereCategory SphereCategory => SphereCategory.World;

        public EnRouteSphere SendViaContainer;



        public override bool AllowDrag
        {
            get { return true; }
        }

        public override bool AllowStackMerge
        {
            get { return true; }
        }

        public override bool AllowAmbientAnimations => true;

        public override float TokenHeartbeatIntervalMultiplier => 1;



        [SerializeField] private TabletopChoreographer _tabletopChoreographer;



        public virtual void Start()
        {
            _background.onClicked += HandleOnTableClicked;
        }


        public override void NotifyTokenInThisSphereInteracted(TokenInteractionEventArgs args)
        {
            if (args.Interaction == Interaction.OnDoubleClicked || args.Interaction == Interaction.OnRightClicked)
                SendTokenToNearestValidDestination(args.Token, new Context(Context.ActionSource.DoubleClickSend));

            base.NotifyTokenInThisSphereInteracted(args);
        }

        private bool SendTokenToNearestValidDestination(Token tokenToSend, Context context)
        {
            if (!tokenToSend.IsValidElementStack())
                return false;


            //   var availableThresholdSpheres = Watchman.Get<HornedAxe>().
            //   GetSpheresOfCategory(SphereCategory.Threshold).Where(s=>s.IsValidDestinationForToken(tokenToSend));

            var allSituations = Watchman.Get<HornedAxe>().GetRegisteredSituations();
            List<Sphere> allCurrentlyOpenSpheresInSituations = new List<Sphere>();
            foreach (var situation in allSituations)
                allCurrentlyOpenSpheresInSituations.AddRange(situation.GetSpheresActiveForCurrentState());
            
            var allThresholdSpheresOpen =
                allCurrentlyOpenSpheresInSituations.Where(sphere => sphere.SphereCategory == SphereCategory.Threshold).ToList();

            var validDestinationSpheres =
                allThresholdSpheresOpen.Where(s => s.IsValidDestinationForToken(tokenToSend));

            

            var selectedItinerary = SelectItineraryForAValidDestinationSphere(tokenToSend, validDestinationSpheres);

            if (selectedItinerary != null)
            {
                if (tokenToSend.Quantity > 1)
                    tokenToSend.CalveToken(tokenToSend.Quantity - 1, new Context(Context.ActionSource.DoubleClickSend));

                tokenToSend.RequestHomingAngelFromCurrentSphere(); //easy to miss this: ensure that we set the homing angel as we would if we'd dragged it

                selectedItinerary = selectedItinerary.WithDuration(NoonConstants.SEND_STACK_TO_SLOT_DURATION);
                selectedItinerary.Depart(tokenToSend,context);


                return true;
            }


            //final fallthrough - couldn't send it anywhere
            return false;
        }

        private static TokenTravelItinerary SelectItineraryForAValidDestinationSphere(Token tokenToSend, IEnumerable<Sphere> validDestinationSpheres)
        {
            TokenTravelItinerary selectedItinerary = null;

            Vector3 selectedTargetDistance = Vector3.positiveInfinity;


            foreach (var thresholdToConsider in validDestinationSpheres)
            {
                TokenTravelItinerary candidateItinerary;
                candidateItinerary = thresholdToConsider.GetItineraryFor(tokenToSend);

                if (thresholdToConsider.GetContainer().IsOpen)
                {
                    selectedItinerary = candidateItinerary;
                    break; //thresholds in open tokens/situations always get priority. This assumes there is only one, though! which may not be the case in future.
                }

                var candidateDistance =
                    candidateItinerary.Anchored3DEndPosition -
                    tokenToSend.Location.Anchored3DPosition; //This might well be wrong / n

                if (candidateDistance.sqrMagnitude < selectedTargetDistance.sqrMagnitude)
                {
                    selectedItinerary = candidateItinerary;
                    selectedTargetDistance = candidateDistance;
                    if (selectedTargetDistance.sqrMagnitude <=
                        0) //we have a valid location, and nothing will be closer than this
                        break;
                }
            }

            return selectedItinerary;
        }


        public override void AcceptToken(Token token, Context context)
        {
          base.AcceptToken(token,context);
       //   token.Stabilise();
          if(token.Shrouded())
              token.Unshroud(true);
          
            SoundManager.PlaySfxOnceThisFrame("CardDrop");
          _tabletopChoreographer.HideAllDebugRects();

          token.HideGhost();


        }


        void HandleOnTableClicked(PointerEventData eventData)
        {
            //Close all open windows if we're not dragging (multi tap stuff)
            // Situation windows get closed first, then details windows.

            var tabletopManager = Watchman.Get<Meniscate>();
            if (tabletopManager.IsSituationWindowOpen())
                tabletopManager.CloseAllSituationWindowsExcept(null);
            else
                Watchman.Get<Notifier>().HideAllDetailWindows();

        }



        // Hide / Show for use with Mansus Map transition
        public virtual void Show(bool show)
        {
            if (show)
                canvasGroupFader.Show();
            else
                canvasGroupFader.Hide();
        }

        public override bool TryDisplayGhost(Token forToken)
        {
            return forToken.DisplayGhostAtChoreographerDrivenPosition(this);

        }
        public override bool DisplayGhostAt(Token forToken, Vector3 overridingWorldPosition)
        {
            return forToken.DisplayGhostAtChoreographerDrivenPosition(this,overridingWorldPosition);
        }
    }

}
