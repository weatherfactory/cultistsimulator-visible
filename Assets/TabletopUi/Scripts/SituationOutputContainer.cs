using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;

public class SituationOutputContainer : MonoBehaviour,ITokenSubscriber
{
    [SerializeField] private SituationWindow situationWindow;

    public void TokenPickedUp(DraggableToken draggableToken)
    {

        var stacks = GetStacksGateway().GetStacks();
        //if no stacks left in output
        if (!stacks.Any())
            situationWindow.DisplayReady();
    }

    public ElementStacksGateway GetStacksGateway()
    {
        return new ElementStacksGateway(new TabletopElementStacksWrapper(this.transform));
    }


    public void TokenInteracted(DraggableToken draggableToken)
    {
        //currently nothing 
    }

    public void TokenReturnedToTabletop(DraggableToken draggableToken, INotification reason)
    {
        //currently nothing; tokens are automatically returned home
    }
}
