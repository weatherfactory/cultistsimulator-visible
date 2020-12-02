using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Entities.Verbs;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Scripts.Spheres.Angels;
using Assets.TabletopUi;
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
    }

    public override void DisplayAndPositionHere(Token token, Context context)
    {

        base.DisplayAndPositionHere(token, context);
        //does a dropzone token exist here?
        Token dropzoneToken = GetAllTokens().FirstOrDefault(t => t.Verb.GetType() == typeof(DropzoneVerb));
        if (dropzoneToken == null)
        {
            //if not, create it
            var dropzoneRecipe = Registry.Get<Compendium>().GetEntityById<Recipe>("dropzone.classic");
            var dropzoneVerb = Registry.Get<Compendium>().GetVerbForRecipe(dropzoneRecipe);
            var tokenLocation = new TokenLocation(Vector3.zero, this.GetPath());

            var cmd = new SituationCreationCommand(dropzoneVerb, dropzoneRecipe, StateEnum.Unstarted, tokenLocation,
                null);
            dropzoneToken = Registry.Get<SituationBuilder>().CreateSituationWithAnchorAndWindow(cmd).GetAnchor();
        }

        //align the incoming token with the dropzone

        _tabletopChoreographer.SnapToGrid(token.transform.localPosition);
    }

    public override void OnTokenInThisSphereInteracted(TokenInteractionEventArgs args)
    {
        if(args.Interaction==Interaction.OnDoubleClicked)
            SendTokenToNearestValidDestination(args.Token);

        base.OnTokenInThisSphereInteracted(args);
    }

    private bool SendTokenToNearestValidDestination(Token tokenToSend)
    {

        if (!tokenToSend.ElementStack.IsValidElementStack())
            return false;


        HashSet<Situation> candidateSituations = new HashSet<Situation>();
        var registeredSituations = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();
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
                candidateDistance = candidateLocation.Position - tokenToSend.Location.Position;

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
                SendViaContainer.PrepareElementForSendAnim(tokenToSend, targetLocation); // this reparents the card so it can animate properly
                SendViaContainer.MoveElementToSituationSlot(tokenToSend, targetLocation, targetThreshold, SEND_STACK_TO_SLOT_DURATION);

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

    public override void AcceptToken(Token token, Context context)
    {
        if(context.actionSource==Context.ActionSource.PlayerDumpAll)
        {
            List<ElementStack> mergeableStacks = GetElementStacks().Where(existing => existing.CanMergeWith(token.ElementStack)).ToList();

            if (mergeableStacks.Count > 0)
                mergeableStacks.First().AcceptIncomingStackForMerge(token.ElementStack);
            else
                base.AcceptToken(token, context);
        }
        else base.AcceptToken(token, context);

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
