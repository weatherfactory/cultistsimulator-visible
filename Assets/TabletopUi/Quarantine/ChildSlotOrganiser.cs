using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// property of DraggableElementToken, used to organise others in workspace
///// </summary>
//public class ChildSlotOrganiser : BoardMonoBehaviour
//{
//    [SerializeField]GameObject PrefabEmptyElementSlot;
//    public void Remove()
//    {
//        foreach (SlotReceiveElement slot in gameObject.GetComponentsInChildren<SlotReceiveElement>())
//        {
//            slot.ClearThisSlot();
//        }
//        BM.ExileToLimboThenDestroy(gameObject);
//    }

//    public void Populate(DraggableElementToken draggedElement)
//    {
//        name = "Organiser for " + draggedElement.Element.Id;
//        foreach (ChildSlotSpecification eachChildSlotSpecification in draggedElement.Element.ChildSlotSpecifications)
//        {
//            GameObject slotObject=Instantiate(PrefabEmptyElementSlot, gameObject.transform, false) as GameObject;
//            SlotReceiveElement childSlot = slotObject.GetComponent<SlotReceiveElement>();
//            childSlot.GoverningChildSlotSpecification = eachChildSlotSpecification;

//        }
//    }
//}
