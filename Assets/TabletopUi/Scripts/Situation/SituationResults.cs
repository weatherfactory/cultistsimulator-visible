#pragma warning disable 0649
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.TabletopUi;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;

public class SituationResults : MonoBehaviour, ITokenContainer {

    public CanvasGroupFader canvasGroupFader;
    [SerializeField] SituationResultsPositioning cardPos;

    private ElementStacksManager stacksManager;

    private SituationController controller;

    public void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
    {
        //do nothing, ever
        incumbentMoved = false;
    }

    public void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
    {
        //do nothing, ever
        incumbentMoved = false;
    }

    public bool AllowDrag { get { return true; } }
    public bool AllowStackMerge { get { return false; } }

    public void Initialise(SituationController sc) {
        controller = sc;
        ITokenTransformWrapper stacksWrapper = new TokenTransformWrapper(transform);
      stacksManager= new ElementStacksManager(stacksWrapper,"situationresults");
    }

    public void Reset() {
        // TODO: Clear out the cards that are still here?
    }

    public void SetOutput(List<IElementStack> allStacksToOutput) {
        if (allStacksToOutput.Any() == false)
            return;       
        GetElementStacksManager().AcceptStacks(allStacksToOutput);

//currently, if the first stack is fresh, we'll turn it over anyway. I think that's OK for now.
        cardPos.ReorderCards(allStacksToOutput);
    }


    public void ElementStackRemovedFromContainer(ElementStackToken elementStackToken)
    {

        elementStackToken.lastTablePos = elementStackToken.transform.position;
        // Did we just drop the last available token? Then reset the state of the window?
        var stacks = GetOutputStacks();
        bool hasStacks = false;

        foreach (var item in stacks)
        {
            if (item != null && item.Defunct == false)
            {
                hasStacks = true;
                break;
            }
        }

        controller.UpdateTokenResultsCountBadge();

        if (!hasStacks)
        {
            controller.SituationHasBeenReset();
            return;
        }

        //// Do some uncovering & repositioning here
        cardPos.ReorderCards(stacks);
    }


    public IEnumerable<IElementStack> GetOutputStacks() {
        return GetElementStacksManager().GetStacks();
    }

    public ElementStacksManager GetElementStacksManager()
    {
        return stacksManager;
    }

    public string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
        return (draggable.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + draggable.RectTransform.localPosition.y).ToString();
    }

    // public to be triggered by button
    public void ShowMap() {
        Registry.Retrieve<MapController>().ShowMansusMap(transform, true);
    }
}
