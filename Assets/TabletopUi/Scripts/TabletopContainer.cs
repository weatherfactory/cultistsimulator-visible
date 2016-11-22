using UnityEngine;
using System.Collections;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Services;

public class TabletopContainer : MonoBehaviour,ITokenSubscriber
{

    [SerializeField] private TabletopManager tabletopManager;

    public void TokenPickedUp(DraggableToken draggableToken)
    {
        ElementStack cardPickedUp = draggableToken as ElementStack;
        if (cardPickedUp != null)
        {
            if (cardPickedUp.Quantity > 1)
            {
                var cardLeftBehind = PrefabFactory.CreateToken<ElementStack>(transform);
                cardLeftBehind.transform.position = draggableToken.transform.position;
                cardLeftBehind.Populate(cardPickedUp.ElementId, cardPickedUp.Quantity - 1);
                cardPickedUp.SetQuantity(1);
            }
        }
    }

    public void TokenReturnedToTabletop(DraggableToken draggableToken, INotification reason)
    {
        tabletopManager.PutOnTable(draggableToken);
    }

    public void TokenInteracted(DraggableToken draggableToken)
    {
        SituationToken box = draggableToken as SituationToken;
        if (box != null)
        {
            if (!box.IsOpen)
                tabletopManager.ShowSituationWindow(box);
            else
                tabletopManager.HideSituationWindow(box);
        }

    }



}
