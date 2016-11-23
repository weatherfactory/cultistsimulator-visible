using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.TabletopUi.Scripts
{
    public class TabletopElementStacksWrapper: IElementStacksWrapper
    {
        private Transform wrappedTransform;
        private ITokenContainer wrappedContainer;

        public TabletopElementStacksWrapper(Transform t)
        {
            wrappedTransform = t;
            wrappedContainer = wrappedTransform.GetComponent<ITokenContainer>();
        }


        public IElementStack ProvisionElementStack(string elementId, int quantity)
        {
            IElementStack stack = PrefabFactory.CreateToken<ElementStack>(wrappedTransform);
            stack.Populate(elementId,quantity);
           return  Accept(stack);
        }

        public IElementStack Accept(IElementStack stack)
        {
            Transform stackTransform = ((ElementStack) stack).transform;
            stackTransform.SetParent(wrappedTransform);
             ((ElementStack)stack).SetContainer(wrappedContainer);
            return stack;
        }


        public IEnumerable<IElementStack> GetStacks()
        {
            return wrappedTransform.GetComponentsInChildren<ElementStack>();
        }
    }
}
