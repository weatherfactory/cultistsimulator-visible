using System.Collections.Generic;
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
    }
}
