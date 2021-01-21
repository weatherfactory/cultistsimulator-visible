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
using SecretHistories.Interfaces;
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

public class TabletopSphere : Sphere,IBeginDragHandler,IEndDragHandler {
#pragma warning disable 649
    [SerializeField] TabletopBackground _background;
    [SerializeField] protected CanvasGroupFader canvasGroupFader;
#pragma warning disable 649

    public override SphereCategory SphereCategory => SphereCategory.World;

    public EnRouteSphere SendViaContainer;



    public override bool AllowDrag { get { return true; } }
    public override bool AllowStackMerge { get { return true; } }
     public override  bool EnforceUniqueStacksInThisContainer => true;

    public override IChoreographer Choreographer
    {
        get { return _tabletopChoreographer; }
    }

    [SerializeField] private TabletopChoreographer _tabletopChoreographer;


    // This is used to determine if this component is the tabletop.
    // Needed because the MapTokenContainer inherits from TabletopTokenContainer but is not the Tabletop
    public virtual bool IsTabletop { get { return true; } }

    public virtual void Start() {
        _background.onClicked += HandleOnTableClicked;
        var dropzoneSpherePath = new SpherePath(NoonConstants.DROPZONE_SPHERE_PATH);
        flock.AddAngel(new TidyAngel(dropzoneSpherePath));
    }


    public override void DisplayAndPositionHere(Token token, Context context)
    {

        base.DisplayAndPositionHere(token, context);

        _tabletopChoreographer.SnapToGrid(token.transform.localPosition);
    }

    public override void OnTokenInThisSphereInteracted(TokenInteractionEventArgs args)
    {
        if(args.Interaction==Interaction.OnDoubleClicked)
            SendTokenToNearestValidDestination(args.Token,new Context(Context.ActionSource.DoubleClickSend));

        base.OnTokenInThisSphereInteracted(args);
    }

    private bool SendTokenToNearestValidDestination(Token tokenToSend,Context context)
    {

        if (!tokenToSend.ElementStack.IsValidElementStack())
            return false;

        var registeredSituations = Watchman.Get<SituationsCatalogue>().GetRegisteredSituations();
        Sphere targetThreshold=null;
        TokenLocation targetLocation = null;
        Vector3 targetDistance = Vector3.positiveInfinity;;

        foreach (Situation candidateSituation in registeredSituations)
        {
            TokenLocation candidateLocation = candidateSituation.GetAnchorLocation();
            Vector3 candidateDistance;
            if (candidateSituation.IsOpen && candidateSituation.Verb.ExclusiveOpen)
                candidateDistance=Vector3.zero;
            else
                candidateDistance = candidateLocation.Anchored3DPosition - tokenToSend.Location.Anchored3DPosition;

            if (candidateDistance.sqrMagnitude < targetDistance.sqrMagnitude)
            {
                targetThreshold = candidateSituation.GetAvailableThresholdsForStackPush(tokenToSend.ElementStack).FirstOrDefault();
                if (targetThreshold != null)
                {
                    targetLocation = candidateLocation;
                    targetDistance = candidateDistance;
                    if (targetDistance == Vector3.zero) //we have a valid location, and nothing will be closer than this
                        break;
                }
            }
        }

        if(targetThreshold!=null)
        {

                if (tokenToSend.ElementQuantity > 1)
                   tokenToSend.CalveToken(tokenToSend.ElementQuantity - 1, new Context(Context.ActionSource.DoubleClickSend));

                TokenTravelItinerary i = new TokenTravelItinerary(tokenToSend.Location,
                        targetLocation)
                    .WithDuration(NoonConstants.SEND_STACK_TO_SLOT_DURATION)
                    .WithScaling(1f, 0.35f)
                    .WithSphereRoute(SendViaContainer, targetThreshold);
                
            i.Depart(tokenToSend,context);
            
            return true;
        }
        

        //final fallthrough - couldn't send it anywhere
        return false;
    }



    // Tabletop specific
    public void CheckOverlappingTokens(Token token) {
        // Verify if we are overlapping with anything. If so: move it.
        _tabletopChoreographer.MoveAllTokensOverlappingWith(token);
    }

    public override void TryMoveAsideFor(Token potentialUsurper, Token incumbent, out bool incumbentMoved) {
        //incumbent.RectTransform.anchoredPosition = GetFreeTokenPos(incumbent);
        incumbentMoved = true;
        AcceptToken(potentialUsurper, new Context(Context.ActionSource.PlayerDrag));
        CheckOverlappingTokens(potentialUsurper);
    }


    void HandleOnTableDropped(PointerEventData eventData)
    {
        //if an anchor or element stack has been dropped on the background, we want to deal with it.
        var potentialToken = eventData.pointerDrag.GetComponent<Token>();
        if(potentialToken!=null)
        {
            potentialToken.SetState(new DroppedInSphereState());
            AcceptToken(potentialToken,
                new Context(Context.ActionSource.PlayerDrag));

            CheckOverlappingTokens(potentialToken);
            SoundManager.PlaySfx("CardDrop");
        }

    }

    void HandleOnTableClicked(PointerEventData eventData) {
        //Close all open windows if we're not dragging (multi tap stuff)
        // Situation windows get closed first, then details windows.

            var tabletopManager = Watchman.Get<TabletopManager>();
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

}
