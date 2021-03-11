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
    public class TabletopSphere : Sphere, IBeginDragHandler, IEndDragHandler
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

        public override bool EnforceUniqueStacksInThisContainer => true;
        public override float TokenHeartbeatIntervalMultiplier => 1;

        public override IChoreographer Choreographer
        {
            get
            {
                if (_tabletopChoreographer == null)
                    return
                        new SimpleChoreographer(); //there's a nasty snarl-up where the choreographer is populated in Awake, which don't play nice with tests
                return _tabletopChoreographer;
            }
        }

        [SerializeField] private TabletopChoreographer _tabletopChoreographer;


        // This is used to determine if this component is the tabletop.
        // Needed because the MapTokenContainer inherits from TabletopTokenContainer but is not the Tabletop
        //YOU AR FUCKING KIDING ME - AK
        public virtual bool IsTabletop
        {
            get { return true; }
        }

        public virtual void Start()
        {
            _background.onClicked += HandleOnTableClicked;
        }


        public override void OnTokenInThisSphereInteracted(TokenInteractionEventArgs args)
        {
            if (args.Interaction == Interaction.OnDoubleClicked)
                SendTokenToNearestValidDestination(args.Token, new Context(Context.ActionSource.DoubleClickSend));

            base.OnTokenInThisSphereInteracted(args);
        }

        private bool SendTokenToNearestValidDestination(Token tokenToSend, Context context)
        {

            if (!tokenToSend.IsValidElementStack())
                return false;

            var allThresholdSpheres = Watchman.Get<HornedAxe>().GetSpheresOfCategory(SphereCategory.Threshold);

            var tokenCurrentPath = tokenToSend.Location.AtSpherePath;

            Sphere targetThreshold = null;
            TokenLocation targetLocation = null;
            Vector3 targetDistance = Vector3.positiveInfinity;
            ;

            foreach (var threshold in allThresholdSpheres)
            {

                Vector3 candidatePosition = threshold.GetReferencePosition(tokenCurrentPath);

                Vector3 candidateDistance;

                candidateDistance = candidatePosition - tokenToSend.Location.Anchored3DPosition;

                if (candidateDistance.sqrMagnitude < targetDistance.sqrMagnitude)
                {
                    targetThreshold = threshold;
                    if (targetThreshold != null)
                    {
                        if (targetDistance.sqrMagnitude <= 0
                        ) //we have a valid location, and nothing will be closer than this
                            break;
                    }
                }
            }

            if (targetThreshold != null)
            {

                if (tokenToSend.Quantity > 1)
                    tokenToSend.CalveToken(tokenToSend.Quantity - 1, new Context(Context.ActionSource.DoubleClickSend));

                TokenTravelItinerary i = new TokenTravelItinerary(tokenToSend.Location,
                        targetLocation)
                    .WithDuration(NoonConstants.SEND_STACK_TO_SLOT_DURATION)
                    .WithScaling(1f, 0.35f)
                    .WithDestinationSpherePath(targetThreshold.GetAbsolutePath());

                i.Depart(tokenToSend, context);

                return true;
            }


            //final fallthrough - couldn't send it anywhere
            return false;
        }



        // Tabletop specific
        public void CheckOverlappingTokens(Token token)
        {
            // Verify if we are overlapping with anything. If so: move it.
            _tabletopChoreographer.MoveAllTokensOverlappingWith(token);
        }

        public override void AcceptToken(Token token, Context context)
        {
          base.AcceptToken(token,context);
          CheckOverlappingTokens(token);
          SoundManager.PlaySfx("CardDrop");
          _tabletopChoreographer.HideAllRects();
        }


        void HandleOnTableClicked(PointerEventData eventData)
        {
            //Close all open windows if we're not dragging (multi tap stuff)
            // Situation windows get closed first, then details windows.

            var tabletopManager = Watchman.Get<TabletopManager>();
            if (tabletopManager.IsSituationWindowOpen())
                tabletopManager.CloseAllSituationWindowsExcept(null);
            else
                tabletopManager.CloseAllDetailsWindows();

        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //can we make the table draggable rather than do the rect scroll thing?
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //can we make the table draggable rather than do the rect scroll thing?
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
            return forToken.DisplayGhost(this);

        }
    }

}
