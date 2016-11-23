using System.Collections.Generic;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
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
        void AcceptStack(IElementStack s);
    }
    public class RecipeSlot : MonoBehaviour, IDropHandler, IRecipeSlot,ITokenContainer
    {

        public event System.Action<RecipeSlot,IElementStack> onCardDropped;
        public event System.Action<IElementStack> onCardPickedUp;
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

        public void AcceptStack(IElementStack s)
        {
            var stack = s as ElementStack;

           GetElementStacksGateway().AcceptStack(s);

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


        public void TokenPickedUp(DraggableToken draggableToken)
        {
            onCardPickedUp(draggableToken as ElementStack);
        }

        public void TokenInteracted(DraggableToken draggableToken)
        {
      
        }

        public bool AllowDrag { get { return true; }}
        public ElementStacksManager GetElementStacksGateway()
        {
            IElementStacksWrapper tabletopStacksWrapper = new ElementStackWrapper(transform);
            return new ElementStacksManager(tabletopStacksWrapper);
        }
    }
}
