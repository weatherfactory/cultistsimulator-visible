using UnityEngine;
using System.Collections;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;

public class SituationStorage : MonoBehaviour,ITokenContainer
{

    public void TokenPickedUp(DraggableToken draggableToken)
    {
        
    }

    public void TokenInteracted(DraggableToken draggableToken)
    {
        
    }

    public bool AllowDrag { get { return false; } }
}
