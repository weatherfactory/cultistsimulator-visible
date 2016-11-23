using System.Collections.Generic;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using Debug = System.Diagnostics.Debug;

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

        public event System.Action<RecipeSlot,IElementStack> onCardDropped;
        public SlotSpecification GoverningSlotSpecification { get; set; }
        public IList<RecipeSlot> childSlots { get; set; }

        // TODO: Needs hover feedback!

        public void Awake()
        {
            childSlots=new List<RecipeSlot>();
        }

        public void OnDrop(PointerEventData eventData) {

            ElementStack stack = DraggableToken.itemBeingDragged as ElementStack;
            if (stack != null)
            {
                    SlotMatchForAspects match = GetSlotMatchForStack(stack);
                    if (match.MatchType == SlotMatchForAspectsType.Okay)
                    {
                        DraggableToken.resetToStartPos = false;
                        // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
                        AcceptStack(stack);
                    }

                    else
                    stack.ReturnToTabletop(new Notification("I can't put that there - ", match.GetProblemDescription()));
                
             }
        }

        public void AcceptStack(ElementStack stack)
        {
            stack.transform.SetParent(transform);
            stack.transform.localPosition = Vector3.zero;
            stack.transform.localRotation = Quaternion.identity;

            Assert.IsNotNull(onCardDropped, "no delegate set for cards dropped on recipe slots");
            // ReSharper disable once PossibleNullReferenceException
            onCardDropped(this, stack);
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
