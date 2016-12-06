using UnityEngine;
using System.Collections;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;

public class SituationStorage : MonoBehaviour,ITokenContainer
{

    public void TokenPickedUp(DraggableToken draggableToken)
    {
        
    }


    public bool AllowDrag { get { return false; } }
    public ElementStacksManager GetElementStacksManager()
    {
        ITokenTransformWrapper tabletopStacksWrapper = new TokenTransformWrapper(transform);
        return new ElementStacksManager(tabletopStacksWrapper);
    }

    public ITokenTransformWrapper GetTokenTransformWrapper()
    {
        return new TokenTransformWrapper(transform);

    }

    public string GetSaveLocationInfoForDraggable(DraggableToken draggable)
    {
        return "slot_storage";
    }
}
