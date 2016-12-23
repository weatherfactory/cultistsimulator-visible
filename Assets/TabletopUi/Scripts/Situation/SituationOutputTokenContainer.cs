using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using TMPro;
using UnityEngine.Assertions;

public class SituationOutputTokenContainer : MonoBehaviour, ITokenContainer {

    private Results _parentResults;

    public void SetParentOutputContainer(Results soc) {
        _parentResults = soc;
    }

    public void TokenPickedUp(DraggableToken draggableToken) {
        Assert.IsNotNull(_parentResults, "parentSituationOutputContainer is null!");

        var stacks = GetElementStacksManager().GetStacks();
        //if no stacks left in output
        if (!stacks.Any()) {
            _parentResults.AllOutputsGone();
            DestroyObject(this.gameObject);
            
        }
    }

    public bool AllowDrag { get { return true; } }
    public bool AllowStackMerge { get { return false; } }

    public ElementStacksManager GetElementStacksManager() {
        ITokenTransformWrapper tabletopStacksWrapper = new TokenTransformWrapper(transform);
        return new ElementStacksManager(tabletopStacksWrapper);
    }

    public string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
        return (draggable.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + draggable.RectTransform.localPosition.y).ToString();
    }
}


