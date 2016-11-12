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
        private Transform transform;
        public TabletopElementStacksWrapper(Transform t)
        {
            transform = t;
        }

        public IElementStack ProvisionElementStack(string elementId, int quantity)
        {
            IElementStack stack = PrefabFactory.CreateTokenWithSubscribers<ElementStack>(transform);
           stack.Populate(elementId,quantity);
            return stack;
        }

        public IEnumerable<IElementStack> Stacks()
        {
            return transform.GetComponentsInChildren<ElementStack>();
        }
    }
}
