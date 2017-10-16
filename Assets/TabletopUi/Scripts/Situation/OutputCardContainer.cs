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

public class OutputCardContainer : MonoBehaviour, ITokenContainer {

    protected SituationController _situationController;

    public void Initialise(SituationController sc) {
        _situationController = sc;
    }

    public void TokenPickedUp(DraggableToken draggableToken) {
        //if (GetComponentInChildren<ElementStackToken>() == null)
        //    _situationController.StartingSlotsUpdated(); // this forces a redraw - does not work cause if you don't drop it properly it is gone
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


