using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;

public class TabletopTokenContainer : AbstractTokenContainer {
#pragma warning disable 649
    [SerializeField] TabletopBackground _background;
    [SerializeField] protected CanvasGroupFader canvasGroupFader;
    #pragma warning disable 649

    protected Choreographer choreo;

    public override bool AllowDrag { get { return true; } }
    public override bool AllowStackMerge { get { return true; } }

    // This is used to determine if this component is the tabletop.
    // Needed because the MapTokenContainer inherits from TabletopTokenContainer but is not the Tabletop
    public virtual bool IsTabletop { get { return true; } }

    public override void Initialise() {
        _elementStacksManager = new ElementStacksManager(this, "tabletop");
        _elementStacksManager.EnforceUniqueStacksInThisStackManager = true; // Martin: This ensures that this stackManager kills other copies when a unique is dropped in

        choreo = Registry.Get<Choreographer>();
        InitialiseListeners();
    }

    private void InitialiseListeners() {
        // Init Listeners to pre-existing DisplayHere Objects
        _background.onDropped += HandleOnTableDropped;
        _background.onClicked += HandleOnTableClicked;
    }

    public override void OnDestroy() {
        base.OnDestroy();

        // Static event so make sure to de-init once this object is destroyed
        }



    public override void DisplayHere(IToken token, Context context) {
        // We're not setting the location; this is used to display a token dragged and dropped to an arbitrary position
        // (or loaded and added to an arbitrary position)
        token.transform.SetParent(transform, true);
        token.transform.localRotation = Quaternion.identity;
		token.SnapToGrid();
        token.SetTokenContainer(this, context);
        token.DisplayAtTableLevel(); // This puts it on the table, so now the choreographer will pick it up
    }

    // Tabletop specific
    public void CheckOverlappingTokens(IToken token) {
        // Verify if we are overlapping with anything. If so: move it.
        choreo.MoveAllTokensOverlappingWith(token);
    }

    public override void TryMoveAsideFor(VerbAnchor potentialUsurper, AbstractToken incumbent, out bool incumbentMoved) {
        //incumbent.RectTransform.anchoredPosition = GetFreeTokenPos(incumbent);
        incumbentMoved = true;
        DisplaySituationTokenOnTable(potentialUsurper, new Context(Context.ActionSource.PlayerDrag));
    }

    public override void TryMoveAsideFor(ElementStackToken potentialUsurper, AbstractToken incumbent, out bool incumbentMoved) {
        // We don't merge here. We assume if we end up here no merge was possible
        //incumbent.RectTransform.anchoredPosition = GetFreeTokenPos(incumbent);
        incumbentMoved = true;
        _elementStacksManager.AcceptStack(potentialUsurper, new Context(Context.ActionSource.PlayerDrag));
        CheckOverlappingTokens(potentialUsurper);
    }

    Vector2 GetFreeTokenPos(AbstractToken incumbent) {
        var choreo = Registry.Get<Choreographer>();
        var currentPos = incumbent.RectTransform.anchoredPosition;

        return choreo.GetFreePosWithDebug(incumbent, currentPos);
    }

    public void DisplaySituationTokenOnTable(ISituationAnchor anchor, Context context) {
        DisplayHere(anchor, context);
        CheckOverlappingTokens(anchor);
        anchor.DisplayAtTableLevel();
    }

    public override string GetSaveLocationInfoForDraggable(AbstractToken @abstract) {
        try
        {

        return (@abstract.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + @abstract.RectTransform.localPosition.y).ToString();
        }
        catch (Exception)
        {
            //in case the token is misbehaving somehow and TRAPPED BETWEEN WORLS
            return 0.ToString() + SaveConstants.SEPARATOR + 0.ToString();
        }

    }



  
    void HandleOnTableDropped() {
        if (HornedAxe.itemBeingDragged != null) {
            HornedAxe.SetReturn(false, "dropped on the background");

            if (HornedAxe.itemBeingDragged is VerbAnchor) {
                DisplaySituationTokenOnTable((VerbAnchor)HornedAxe.itemBeingDragged, new Context(Context.ActionSource.PlayerDrag));
            }
            else if (HornedAxe.itemBeingDragged is ElementStackToken) {
                GetElementStacksManager().AcceptStack(((ElementStackToken)HornedAxe.itemBeingDragged),
                    new Context(Context.ActionSource.PlayerDrag));
            }
            else {
                throw new NotImplementedException("Tried to put something weird on the table");
            }

            CheckOverlappingTokens(HornedAxe.itemBeingDragged);
            SoundManager.PlaySfx("CardDrop");
        }
    }

    void HandleOnTableClicked() {
        //Close all open windows if we're not dragging (multi tap stuff)
        // Situation windows get closed first, then details windows.
        if (HornedAxe.itemBeingDragged == null)
        {
            var tabletopManager = Registry.Get<TabletopManager>();
            if (tabletopManager.IsSituationWindowOpen())
                tabletopManager.CloseAllSituationWindowsExcept(null);
            else
                tabletopManager.CloseAllDetailsWindows();
        }
    }

    public void HandleDragStateChanged(bool isDragging) {
        var draggedElement = HornedAxe.itemBeingDragged as ElementStackToken;

        if (draggedElement != null)
            ShowDestinationsForStack(draggedElement, isDragging);
    }

    private void ShowDestinationsForStack(ElementStackToken draggedElement, bool show)
	{
        // Stacks on Tabletop
        var tabletopStacks = GetElementStacksManager().GetStacks();
        ElementStackToken token;

        foreach (var card in tabletopStacks)
		{
            if (card.EntityId != draggedElement.EntityId || card.Defunct)
                continue;

            if (!show || card.AllowsIncomingMerge())
			{
                token = card as ElementStackToken;

                if (token != null && draggedElement.CanMergeWith(token)) {
                    token.SetGlowColor(UIStyle.TokenGlowColor.Default);
                    token.ShowGlow(show, false);
                }
            }
        }

        // Situations on Tabletop
        var situations = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

        foreach (var sit in situations)
		{
            if (sit.IsOpen)
                sit.ShowDestinationsForStack(draggedElement, show);

            sit.ShowVisualEffectIfCanTakeDroppedToken(draggedElement,show);

        }
    }


    // Returns a rect for use by the Choreographer
    public Rect GetRect() {
        var rectTrans = transform as RectTransform;
        return rectTrans.rect;
    }

    // Returns all visual tokens for use by the Choreographer
    public virtual IEnumerable<AbstractToken> GetTokens() {
        return transform.GetComponentsInChildren<AbstractToken>();
    }

    // Hide / Show for use with Mansus Map transition
    public virtual void Show(bool show) {
        if (show)
            canvasGroupFader.Show();
        else
            canvasGroupFader.Hide();
    }

}
