using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlotsPanel : MonoBehaviour
{
    [SerializeField]
    private SlotReceiveElement prefabEmptyElementSlot;

    [SerializeField] private TimerPanel ParentTimerPanel;

    public void DisplaySlotsForRecipe(List<ChildSlotSpecification> cssList)
    {
        foreach(var css in cssList)
            { 

            SlotReceiveElement slot=Instantiate(prefabEmptyElementSlot,transform,false) as SlotReceiveElement;
            slot.AddSubscriber(ParentTimerPanel);
                slot.GoverningChildSlotSpecification = css;
            }
    }

    

    public Dictionary<string, int> GrabAndConsumeElements()
    {
       var elementSlots= GetComponentsInChildren<SlotReceiveElement>();

        Dictionary<string,int> changes=new Dictionary<string, int>();
        foreach(var s in elementSlots)
            if(s.GetTokenInSlot()!=null)
            changes.Add(s.GetTokenInSlot().Element.Id,1);

        
        foreach (SlotReceiveElement s in elementSlots)
       DestroyObject(s.gameObject);

        return changes;

    }

    
}
