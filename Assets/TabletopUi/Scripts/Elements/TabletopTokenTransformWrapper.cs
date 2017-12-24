using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Elements
{
    /// <summary>
    /// override of the basic transform wrapper for the tabletop containsTokens
    /// </summary>
    public class TabletopTokenTransformWrapper:TokenTransformWrapper
    {
        public TabletopTokenTransformWrapper(Transform t) : base(t)
        {
        }

        public override void DisplayHere(DraggableToken token)
        {
            Transform stackTransform = token.transform;
            //we're not changing the location; this Accept is used to accept a token dragged and dropped to an arbitrary position
            //(or loaded and added to an arbitrary position)
            stackTransform.SetParent(wrappedTransform,true);
            stackTransform.localRotation = Quaternion.identity;
            token.SetContainer(containsTokens);
        }



    }
}
