using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;

public class SituationOutputContainer : MonoBehaviour,ITokenContainer
{
    [SerializeField] private SituationWindow situationWindow;

    public void TokenPickedUp(DraggableToken draggableToken)
    {

        var stacks = GetElementStacksGateway().GetStacks();
        //if no stacks left in output
        if (!stacks.Any())
            situationWindow.DisplayStarting();
    }




    public void TokenInteracted(DraggableToken draggableToken)
    {
        //currently nothing 
    }

    public bool AllowDrag { get { return true; } }
    public ElementStacksManager GetElementStacksGateway()
    {
        IElementStacksWrapper tabletopStacksWrapper = new ElementStackWrapper(transform);
        return new ElementStacksManager(tabletopStacksWrapper);
    }
}
