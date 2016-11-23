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

    public void TokenInteracted(DraggableToken draggableToken)
    {
        
    }

    public bool AllowDrag { get { return false; } }
    public ElementStacksGateway GetElementStacksGateway()
    {
        IElementStacksWrapper tabletopStacksWrapper = new ElementStackWrapper(transform);
        return new ElementStacksGateway(tabletopStacksWrapper);
    }
}
