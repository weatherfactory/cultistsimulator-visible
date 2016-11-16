using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.TabletopUi.Scripts
{
    public class TabletopElementStacksWrapper: IElementStacksWrapper
    {
        private Transform wrappedTransform;
        public TabletopElementStacksWrapper(Transform t)
        {
            wrappedTransform = t;
        }

        public IElementStack ProvisionElementStack(string elementId, int quantity)
        {
            IElementStack stack = PrefabFactory.CreateTokenWithSubscribers<ElementStack>(wrappedTransform);
            stack.Populate(elementId,quantity);
            return stack;
        }

        public void Accept(IElementStack stack)
        {
            Transform stackTransform = (stack as ElementStack).transform;
            stackTransform.SetParent(wrappedTransform);
        }


        public IEnumerable<IElementStack> GetStacks()
        {
            return wrappedTransform.GetComponentsInChildren<ElementStack>();
        }
    }
}
