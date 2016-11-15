using System.Collections.Generic;
using Assets.Core.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.CS.TabletopUI
{
    public class RecipeSlot : MonoBehaviour, IDropHandler {

        public event System.Action<RecipeSlot> onCardDropped;
        public ChildSlotSpecification GoverningSlotSpecification;
        public List<RecipeSlot> childSlots;

        // TODO: Needs hover feedback!

        public void OnDrop(PointerEventData eventData) {
            if (onCardDropped != null)
                onCardDropped(this);
        }

        public ElementStack GetElementStackInSlot()
        {
            return GetComponentInChildren<ElementStack>();
        }

        public SlotMatchForAspects GetSlotMatchForStack(ElementStack stack)
        {
            if (GoverningSlotSpecification == null)
                return SlotMatchForAspects.MatchOK();
            else
                return GoverningSlotSpecification.GetSlotMatchForAspects(stack.GetAspects());
        }
    }
}
