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
        IElementStack GetElementStackInSlot();
        DraggableToken GetTokenInSlot();
        SlotMatchForAspects GetSlotMatchForStack(IElementStack stack);
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

        public bool HasChildSlots()
        {
            if (childSlots == null)
                return false;
            return childSlots.Count > 0;
        }
        public void OnDrop(PointerEventData eventData) {

            IElementStack stack = DraggableToken.itemBeingDragged as IElementStack;
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

        public void AcceptStack(IElementStack stack)
        {

           GetElementStacksManager().AcceptStack(stack);

            Assert.IsNotNull(onCardDropped, "no delegate set for cards dropped on recipe slots");
            // ReSharper disable once PossibleNullReferenceException
            onCardDropped(this, stack);
        }

        public DraggableToken GetTokenInSlot()
        {
            return GetComponentInChildren<DraggableToken>();
        }
        public IElementStack GetElementStackInSlot()
        {
            return GetComponentInChildren<IElementStack>();
        }

        public SlotMatchForAspects GetSlotMatchForStack(IElementStack stack)
        {
            if (GoverningSlotSpecification == null)
                return SlotMatchForAspects.MatchOK();
            else
                return GoverningSlotSpecification.GetSlotMatchForAspects(stack.GetAspects());
        }


        public void TokenPickedUp(DraggableToken draggableToken)
        {
            onCardPickedUp(draggableToken as IElementStack);
        }

        public void TokenInteracted(DraggableToken draggableToken)
        {
      
        }

        public bool AllowDrag { get { return true; }}
        public ElementStacksManager GetElementStacksManager()
        {
            ITokenTransformWrapper tabletopStacksWrapper = new TokenTransformWrapper(transform);
            return new ElementStacksManager(tabletopStacksWrapper);
        }

        public ITokenTransformWrapper GetTokenTransformWrapper()
        {
            return new TokenTransformWrapper(transform);
        }
    }
}
