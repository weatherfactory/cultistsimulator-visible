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
        private ITokenSubscriber wrappedContainer;

        public TabletopElementStacksWrapper(Transform t)
        {
            wrappedTransform = t;
        }

        public TabletopElementStacksWrapper(Transform t,ITokenSubscriber s)
        {
            wrappedTransform = t;
            wrappedContainer = s;
        }


        public IElementStack ProvisionElementStack(string elementId, int quantity)
        {
            IElementStack stack = PrefabFactory.CreateTokenWithSubscribers<ElementStack>(wrappedTransform);
            stack.Populate(elementId,quantity);
            if(wrappedContainer!=null)
                ((ElementStack) stack).SetContainer(wrappedContainer);

            return stack;
        }

        public void Accept(IElementStack stack)
        {
            Transform stackTransform = ((ElementStack) stack).transform;
            stackTransform.SetParent(wrappedTransform);
            if (wrappedContainer != null)
                ((ElementStack)stack).SetContainer(wrappedContainer);
        }


        public IEnumerable<IElementStack> GetStacks()
        {
            return wrappedTransform.GetComponentsInChildren<ElementStack>();
        }
    }
}
