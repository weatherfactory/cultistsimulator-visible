using UnityEngine;
using System.Collections;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using System;

public class SituationStorage : MonoBehaviour, ITokenContainer {
    public void TryMoveAsideFor(DraggableToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
    {
        //I don't *think* this should ever be called. Let's find out.
        //if it's not, ofc, we have one too few interfaces. The ITokenContainer is being used as both 'thing that has a stacksmanager' and 'direct parent that determines behaviour'
        throw new NotImplementedException();
    }

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
