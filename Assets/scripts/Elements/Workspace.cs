using UnityEngine;
using System.Collections;

public class Workspace : BoardMonoBehaviour
{

    [SerializeField] private SlotReceiveVerb VerbSlot;
    private GameObject RootElementSlot;

    public bool IsRootElementPresent { get { return RootElementSlot != null; } }
    public string GetCurrentVerbId()
    {
        return VerbSlot.GetCurrentVerbId();
    }

    public DraggableElementToken[] GetCurrentElements()
    {
        return GetComponentsInChildren<DraggableElementToken>();
    }

    public void MakeFirstSlotAvailable(Vector3 governorPosition,GameObject prefabEmptyElementSlot)
    {
        if(!IsRootElementPresent)
        { 
        int governedStepRight = 50;
        int nudgeDown = -10;
       RootElementSlot = Instantiate(prefabEmptyElementSlot, transform, false) as GameObject;
        Vector3 newSlotPosition = new Vector3(governorPosition.x + governedStepRight, governorPosition.y + nudgeDown);
        RootElementSlot.transform.localPosition = newSlotPosition;
        }
    }
}
