using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Elements
{
    public class TabletopTokenTransformWrapper:TokenTransformWrapper
    {
        public TabletopTokenTransformWrapper(Transform t) : base(t)
        {
        }

        public override void Accept(DraggableToken token)
        {
            
            Transform stackTransform = token.transform;

            stackTransform.SetParent(wrappedTransform,true);
            stackTransform.localRotation = Quaternion.identity;

          token.SetContainer(wrappedContainer);
        }
    
    }
}
