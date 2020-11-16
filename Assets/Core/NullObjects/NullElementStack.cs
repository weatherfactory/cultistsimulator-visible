using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;

namespace Assets.TabletopUi.Scripts.Elements
{
    public class NullElementStack: ElementStack
    {
        public NullElementStack()
        {
            Element=new NullElement();
        }
        
        public override bool CanMergeWith(ElementStack intoStack)
        {
            return false;
        }
    }
}
