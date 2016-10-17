using System;
using UnityEngine;
using System.Collections;

public class ChildSlotOrganiser : BoardMonoBehaviour
{
    [SerializeField]GameObject PrefabEmptyElementSlot;
    public void Remove()
    {
        foreach (SlotReceiveElement slot in gameObject.GetComponentsInChildren<SlotReceiveElement>())
        {
            slot.ClearThisSlot();
        }
        BM.ExileToLimboThenDestroy(gameObject);
    }

    public void Populate(DraggableElementToken draggedElement)
    {
        name = "Organiser for " + draggedElement.Element.Id;
        foreach (ChildSlot ci in draggedElement.Element.ChildSlots)
        {
            Instantiate(PrefabEmptyElementSlot, gameObject.transform, false);

        }
    }
}
