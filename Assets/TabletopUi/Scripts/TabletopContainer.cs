using UnityEngine;
using System.Collections;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Services;

public class TabletopContainer : MonoBehaviour,ITokenContainer
{

    [SerializeField] private TabletopManager tabletopManager;

    public void TokenPickedUp(DraggableToken draggableToken)
    {

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

    public bool AllowDrag { get { return true; } }
}
