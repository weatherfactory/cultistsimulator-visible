using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Interfaces;
using Assets.TabletopUi;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts.Infrastructure;

public class SituationResults : MonoBehaviour, ITokenContainer {

    public CanvasGroupFader canvasGroupFader;

    private SituationWindow window;
    private SituationController controller;

    public bool AllowDrag { get { return true; } }
    public bool AllowStackMerge { get { return false; } }

    public void Initialise(SituationWindow w, SituationController sc) {
        window = w;
        controller = sc;
    }

    public void Reset() {
        // Clear out the cards that are still here?
    }

    public void SetOutput(List<IElementStack> stacks) {
        if (stacks.Any() == false)
            return;

        GetElementStacksManager().AcceptStacks(stacks);

        // TODO: Arrange things visually here
    }


    public void TokenPickedUp(DraggableToken draggableToken) {
    }

    public void TokenDropped(DraggableToken draggableToken) {
        // Did we just drop the last available token? Then reset the state of the window
        // TODO: Should we do this in the controller instead? via StartingSlotsUpdated()?
        if (GetElementStacksManager().GetStacks().Any())
            window.SetUnstarted();
    }

    public IEnumerable<IElementStack> GetOutputStacks() {
        return GetElementStacksManager().GetStacks();
    }

    public ElementStacksManager GetElementStacksManager() {
        ITokenTransformWrapper stacksWrapper = new TokenTransformWrapper(transform);
        return new ElementStacksManager(stacksWrapper);
    }

    public string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
        return (draggable.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + draggable.RectTransform.localPosition.y).ToString();
    }
}
