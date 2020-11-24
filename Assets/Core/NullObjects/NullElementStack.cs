using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Noon;

namespace Assets.TabletopUi.Scripts.Elements
{
    public class NullElementStack: ElementStack
    {
        public override Element Element
        {
            get
            {
                return new NullElement();
            }
            set
            {
                NoonUtility.LogWarning("Can't set an element for a NullElementStack, not even " + value.Id);
            }

        }

        public override bool CanMergeWith(ElementStack intoStack)
        {
            return false;
        }
    }
}
