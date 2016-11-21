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
        private ITokenSubscriber wrappedSubscriber;

        public TabletopElementStacksWrapper(Transform t)
        {
            wrappedTransform = t;
        }

        public TabletopElementStacksWrapper(Transform t,ITokenSubscriber s)
        {
            wrappedTransform = t;
            wrappedSubscriber = s;
        }


        public IElementStack ProvisionElementStack(string elementId, int quantity)
        {
            IElementStack stack = PrefabFactory.CreateTokenWithSubscribers<ElementStack>(wrappedTransform);
            stack.Populate(elementId,quantity);
            if(wrappedSubscriber!=null)
                ((ElementStack) stack).Subscribe(wrappedSubscriber);

            return stack;
        }

        public void Accept(IElementStack stack)
        {
            Transform stackTransform = ((ElementStack) stack).transform;
            stackTransform.SetParent(wrappedTransform);
            if (wrappedSubscriber != null)
                ((ElementStack)stack).Subscribe(wrappedSubscriber);
        }


        public IEnumerable<IElementStack> GetStacks()
        {
            return wrappedTransform.GetComponentsInChildren<ElementStack>();
        }
    }
}
