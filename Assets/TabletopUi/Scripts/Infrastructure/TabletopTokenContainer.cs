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

    public override bool AllowDrag { get { return true; } }
    public override bool AllowStackMerge { get { return true; } }

    public override void Initialise() {
        _elementStacksManager = new ElementStacksManager(this, "tabletop");
        _elementStacksManager.EnforceUniqueStacks = true; // Martin: This ensures that this stackManager kills other copies when a unique is dropped in 
    }

    public override void DisplayHere(DraggableToken token) {
        // We're not setting the location; this is used to display a token dragged and dropped to an arbitrary position
        // (or loaded and added to an arbitrary position)
        token.transform.SetParent(transform, true);
        token.transform.localRotation = Quaternion.identity;
        token.SetViewContainer(this);
        token.DisplayAtTableLevel();
    }

    public override void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved) {
        incumbent.GetRectTransform().anchoredPosition = GetFreePosIgnoringCurrentPos(incumbent);
        incumbentMoved = true;
        DisplaySituationTokenOnTable(potentialUsurper);
    }

    public override void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved) {
        // We don't merge here. We assume if we end up here no merge was possible
        incumbent.GetRectTransform().anchoredPosition = GetFreePosIgnoringCurrentPos(incumbent);
        incumbentMoved = true;
        _elementStacksManager.AcceptStack(potentialUsurper);
    }

    Vector2 GetFreePosIgnoringCurrentPos(DraggableToken incumbent) {
        var choreo = Registry.Retrieve<Choreographer>();
        var currentPos = incumbent.GetRectTransform().anchoredPosition;
        var ignorePositions = new Rect[1] { incumbent.GetRectTransform().rect }; // this is to ignore the current pos.

        return choreo.GetFreeTokenPositionWithDebug(incumbent, currentPos, ignorePositions);
    }

    public ISituationAnchor CreateSituation(SituationCreationCommand creationCommand, string locatorInfo = null) {
        return Registry.Retrieve<SituationBuilder>().CreateTokenWithAttachedControllerAndSituation(creationCommand, locatorInfo);
    }

    public void DisplaySituationTokenOnTable(SituationToken token) {
        DisplayHere(token);
        token.DisplayAtTableLevel();
    }

    public override string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
        return (draggable.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + draggable.RectTransform.localPosition.y).ToString();
    }

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
