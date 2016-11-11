using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlotsPanel : MonoBehaviour
{
    [SerializeField]
    private SlotReceiveElement prefabEmptyElementSlot;

    public void DisplaySlotsForRecipe(List<ChildSlotSpecification> cssList)
    {
        foreach(var css in cssList)
            { 

            SlotReceiveElement slot=Instantiate(prefabEmptyElementSlot,transform,false) as SlotReceiveElement;
                slot.GoverningChildSlotSpecification = css;
            }
    }
}
