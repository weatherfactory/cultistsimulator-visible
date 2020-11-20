using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts.TokenContainers;
using Noon;
using UnityEngine.EventSystems;

public class TabletopSphere : Sphere,IBeginDragHandler,IEndDragHandler {
#pragma warning disable 649
    [SerializeField] TabletopBackground _background;
    [SerializeField] protected CanvasGroupFader canvasGroupFader;
#pragma warning disable 649

    public override SphereCategory SphereCategory => SphereCategory.World;

    public EnRouteSphere SendViaContainer;
    public const float SEND_STACK_TO_SLOT_DURATION = 0.2f;


    public override bool AllowDrag { get { return true; } }
    public override bool AllowStackMerge { get { return true; } }
     public override  bool EnforceUniqueStacksInThisContainer => true;

     protected override Vector3 GetDefaultPositionForProvisionedToken(Token token)
     {
         return Registry.Get<Choreographer>().GetFreePosWithDebug(token, Vector3.zero);
     }
         

    // This is used to determine if this component is the tabletop.
    // Needed because the MapTokenContainer inherits from TabletopTokenContainer but is not the Tabletop
    public virtual bool IsTabletop { get { return true; } }

    public virtual void Start() {
        InitialiseListeners();
        _notifiersForContainer.Add(Registry.Get<INotifier>());
        }

    private void InitialiseListeners() {
        // Init Listeners to pre-existing DisplayHere Objects
        _background.onDropped += HandleOnTableDropped;
        _background.onClicked += HandleOnTableClicked;
    }

    public override void DisplayHere(Token token, Context context) {

        base.DisplayHere(token, context);
		token.SnapToGrid();
    }

    public override void OnTokenDoubleClicked(TokenEventArgs args)
    {
        SendTokenToNearestValidDestination(args.Token);
        base.OnTokenDoubleClicked(args);
    }

    private bool SendTokenToNearestValidDestination(Token tokenToSend)
    {

        if (!tokenToSend.ElementStack.IsValidElementStack())
            return false;


        Dictionary<Sphere, Situation> candidateThresholds = new Dictionary<Sphere, Situation>();
        var registeredSituations = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();
        foreach (Situation situation in registeredSituations)
        {
            try
            {
                var candidateThreshold = situation.GetFirstAvailableThresholdForStackPush(tokenToSend.ElementStack);
                candidateThresholds.Add(candidateThreshold, situation);

            }
            catch (Exception e)
            {
                NoonUtility.LogWarning("Problem adding a candidate threshold to list of valid thresholds - does a valid threshold belong to more than one situation? - " + e.Message);
            }
        }

        if (candidateThresholds.Any())
        {
            Sphere selectedCandidate = null;
            float selectedSlotDist = float.MaxValue;

            foreach (Sphere candidate in candidateThresholds.Keys)
            {
                Vector3 distance = candidateThresholds[candidate].GetAnchorLocation().Position - transform.position;
                //Debug.Log("Dist to " + tokenpair.Token.EntityId + " = " + dist.magnitude );
                if (candidateThresholds[candidate].IsOpen && candidateThresholds[candidate].Verb.ExclusiveOpen)
                    distance = Vector3.zero;    // Prioritise open windows above all else
                if (distance.sqrMagnitude < selectedSlotDist)
                {
                    selectedSlotDist = distance.sqrMagnitude;
                    selectedCandidate = candidate;
                }
            }

            if (selectedCandidate != null)
            {

                var candidateAnchorLocation = candidateThresholds[selectedCandidate].GetAnchorLocation();
                if (tokenToSend.ElementQuantity > 1)
                   tokenToSend.CalveTokenAtSamePosition(tokenToSend.ElementQuantity - 1, new Context(Context.ActionSource.DoubleClickSend));
                SendViaContainer.PrepareElementForSendAnim(tokenToSend, candidateAnchorLocation); // this reparents the card so it can animate properly
                SendViaContainer.MoveElementToSituationSlot(tokenToSend, candidateAnchorLocation, selectedCandidate, SEND_STACK_TO_SLOT_DURATION);

                return true;
            }
        }

        //final fallthrough - couldn't send it anywhere
        return false;
    }



    // Tabletop specific
    public void CheckOverlappingTokens(Token token) {
        // Verify if we are overlapping with anything. If so: move it.
        Registry.Get<Choreographer>().MoveAllTokensOverlappingWith(token);
    }

    public override void TryMoveAsideFor(Token potentialUsurper, Token incumbent, out bool incumbentMoved) {
        //incumbent.RectTransform.anchoredPosition = GetFreeTokenPos(incumbent);
        incumbentMoved = true;
        AcceptToken(potentialUsurper, new Context(Context.ActionSource.PlayerDrag));
        CheckOverlappingTokens(potentialUsurper);
    }


    Vector2 GetFreeTokenPos(Token incumbent) {
        var choreo = Registry.Get<Choreographer>();
        var currentPos = incumbent.TokenRectTransform.anchoredPosition;

        return choreo.GetFreePosWithDebug(incumbent, currentPos);
    }


  
    void HandleOnTableDropped(PointerEventData eventData)
    {
        //if an anchor or element stack has been dropped on the background, we want to deal with it.
        var potentialToken = eventData.pointerDrag.GetComponent<Token>();
        if(potentialToken!=null)
        {
            potentialToken.SetXNess(TokenXNess.DroppedOnTableContainer);
            AcceptToken(potentialToken,
                new Context(Context.ActionSource.PlayerDrag));

            CheckOverlappingTokens(potentialToken);
            SoundManager.PlaySfx("CardDrop");
        }

    }

    void HandleOnTableClicked(PointerEventData eventData) {
        //Close all open windows if we're not dragging (multi tap stuff)
        // Situation windows get closed first, then details windows.

            var tabletopManager = Registry.Get<TabletopManager>();
            if (tabletopManager.IsSituationWindowOpen())
                tabletopManager.CloseAllSituationWindowsExcept(null);
            else
                tabletopManager.CloseAllDetailsWindows();
        
    }

    public void OnBeginDrag(PointerEventData eventData) {
 //can we make the table draggable rather than do the rect scroll thing?
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //can we make the table draggable rather than do the rect scroll thing?
    }



    // Hide / Show for use with Mansus Map transition
    public virtual void Show(bool show) {
        if (show)
            canvasGroupFader.Show();
        else
            canvasGroupFader.Hide();
    }

}
