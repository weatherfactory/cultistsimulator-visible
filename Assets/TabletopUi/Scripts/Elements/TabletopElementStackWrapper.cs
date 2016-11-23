using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Elements
{
    public class TabletopElementStackWrapper:ElementStackWrapper
    {
        public TabletopElementStackWrapper(Transform t) : base(t)
        {
        }

        public override IElementStack Accept(IElementStack stack)
        {
            
            Transform stackTransform = ((ElementStack)stack).transform;
            var oldPosition = stackTransform.position;

            stackTransform.SetParent(wrappedTransform,true);
            stackTransform.localRotation = Quaternion.identity;

            ((ElementStack)stack).SetContainer(wrappedContainer);
            return stack;
        }
    
    }
}
