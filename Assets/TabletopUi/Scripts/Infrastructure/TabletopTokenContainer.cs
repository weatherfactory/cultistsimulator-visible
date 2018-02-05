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

    [SerializeField] TabletopBackground _background;

    protected Choreographer choreo;

    public override bool AllowDrag { get { return true; } }
    public override bool AllowStackMerge { get { return true; } }

    public override void Initialise() {
        _elementStacksManager = new ElementStacksManager(this, "tabletop");
        _elementStacksManager.EnforceUniqueStacks = true; // Martin: This ensures that this stackManager kills other copies when a unique is dropped in 

        choreo = Registry.Retrieve<Choreographer>();
        InitialiseListeners();
    }

    private void InitialiseListeners() {
        // Init Listeners to pre-existing DisplayHere Objects
        _background.onDropped += HandleOnTableDropped;
        _background.onClicked += HandleOnTableClicked;
        DraggableToken.onChangeDragState += HandleDragStateChanged;
    }

    public override void OnDestroy() {
        base.OnDestroy();

        // Static event so make sure to de-init once this object is destroyed
        DraggableToken.onChangeDragState -= HandleDragStateChanged;
    }

    #region -- AbstractTokenContainer -------------------------------------------

    public override void DisplayHere(DraggableToken token) {
        // We're not setting the location; this is used to display a token dragged and dropped to an arbitrary position
        // (or loaded and added to an arbitrary position)
        token.transform.SetParent(transform, true);
        token.transform.localRotation = Quaternion.identity;
        token.SetTokenContainer(this);
        token.DisplayAtTableLevel(); // This puts it on the table, so now the choreographer will pick it up
    }

    // Tabletop specific
    public void CheckOverlappingTokens(DraggableToken token) {
        // Verify if we are overlapping with anything. If so: move it.
        choreo.MoveAllTokensOverlappingWith(token);
    }

    public override void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved) {
        //incumbent.RectTransform.anchoredPosition = GetFreeTokenPos(incumbent);
        incumbentMoved = true;
        DisplaySituationTokenOnTable(potentialUsurper);
    }

    public override void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved) {
        // We don't merge here. We assume if we end up here no merge was possible
        //incumbent.RectTransform.anchoredPosition = GetFreeTokenPos(incumbent);
        incumbentMoved = true;
        _elementStacksManager.AcceptStack(potentialUsurper);
        CheckOverlappingTokens(potentialUsurper);
    }

    Vector2 GetFreeTokenPos(DraggableToken incumbent) {
        var choreo = Registry.Retrieve<Choreographer>();
        var currentPos = incumbent.RectTransform.anchoredPosition;

        return choreo.GetFreePosWithDebug(incumbent, currentPos);
    }

    public void DisplaySituationTokenOnTable(SituationToken token) {
        DisplayHere(token);
        CheckOverlappingTokens(token);
        token.DisplayAtTableLevel();
    }

    public override string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
        return (draggable.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + draggable.RectTransform.localPosition.y).ToString();
    }

    #endregion

    #region -- Drag Reactions & Highlighting -------------------------------------------

    void HandleOnTableDropped() {
        if (DraggableToken.itemBeingDragged != null) {
            DraggableToken.SetReturn(false, "dropped on the background");

            if (DraggableToken.itemBeingDragged is SituationToken) 
                DisplaySituationTokenOnTable((SituationToken)DraggableToken.itemBeingDragged);
            else if (DraggableToken.itemBeingDragged is ElementStackToken) 
                GetElementStacksManager().AcceptStack(((ElementStackToken)DraggableToken.itemBeingDragged));
            else 
                throw new NotImplementedException("Tried to put something weird on the table");

            CheckOverlappingTokens(DraggableToken.itemBeingDragged);
            SoundManager.PlaySfx("CardDrop");
        }
    }

    void HandleOnTableClicked() {
        //Close all open windows if we're not dragging (multi tap stuff)
        if (DraggableToken.itemBeingDragged == null)
            Registry.Retrieve<TabletopManager>().CloseAllSituationWindowsExcept(null);
    }

    private void HandleDragStateChanged(bool isDragging) {
        var draggedElement = DraggableToken.itemBeingDragged as ElementStackToken;

        if (draggedElement != null)
            ShowDestinationsForStack(draggedElement, isDragging);
    }

    private void ShowDestinationsForStack(ElementStackToken draggedElement, bool show) {
        // Stacks on Tabletop
        var tabletopStacks = GetElementStacksManager().GetStacks();
        ElementStackToken token;

        foreach (var card in tabletopStacks) {
            if (card.Id != draggedElement.Id || card.Defunct)
                continue;

            if (!show || card.AllowsMerge()) {
                token = card as ElementStackToken;

                if (token != null && token != draggedElement) {
                    token.SetGlowColor(UIStyle.TokenGlowColor.Default);
                    token.ShowGlow(show, false);
                }
            }
        }

        // Situations on Tabletop
        var situations = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();

        foreach (var sit in situations) {
            if (sit.IsOpen)
                sit.ShowDestinationsForStack(draggedElement, show);

            sit.situationToken.SetGlowColor(UIStyle.TokenGlowColor.Default);
            sit.situationToken.ShowGlow(show && sit.CanTakeDroppedToken(draggedElement));
        }
    }

    #endregion

    // Returns a rect for use by the Choreographer
    public Rect GetRect() {
        var rectTrans = transform as RectTransform;
        return rectTrans.rect;
    }

    // Returns all visual tokens for use by the Choreographer
    public virtual IEnumerable<DraggableToken> GetTokens() {
        return transform.GetComponentsInChildren<DraggableToken>();
    }

}
