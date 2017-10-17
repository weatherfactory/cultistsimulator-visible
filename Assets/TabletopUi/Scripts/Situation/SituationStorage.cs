using UnityEngine;
using System.Collections;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using System;

public class SituationStorage : MonoBehaviour, ITokenContainer {

    public bool AllowDrag { get { return false; } }
    public bool AllowStackMerge { get { return false; } }

    public void TokenPickedUp(DraggableToken draggableToken) { }
    public void TokenDropped(DraggableToken draggableToken) { }

    public ElementStacksManager GetElementStacksManager() {
        ITokenTransformWrapper tabletopStacksWrapper = new TokenTransformWrapper(transform);
        return new ElementStacksManager(tabletopStacksWrapper);
    }

    public string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
        return "slot_storage";
    }
}
