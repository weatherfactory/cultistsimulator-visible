using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.CS.TabletopUI
{
    public interface IRecipeSlot
    {
        ElementStack GetElementStackInSlot();
        SlotMatchForAspects GetSlotMatchForStack(ElementStack stack);
        SlotSpecification GoverningSlotSpecification { get; set; }
    }
    public class RecipeSlot : MonoBehaviour, IDropHandler, IRecipeSlot
    {

        public event System.Action<RecipeSlot> onCardDropped;
        public SlotSpecification GoverningSlotSpecification { get; set; }
        public IList<RecipeSlot> childSlots { get; set; }

        // TODO: Needs hover feedback!

        public void Awake()
        {
            childSlots=new List<RecipeSlot>();
        }

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
