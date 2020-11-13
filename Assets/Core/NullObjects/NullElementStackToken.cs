using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;

namespace Assets.TabletopUi.Scripts.Elements
{
    public class NullElementStackToken: ElementStackToken
    {
        public NullElementStackToken()
        {
            _element=new NullElement();
        }
        
        public override bool CanMergeWith(ElementStackToken intoStack)
        {
            return false;
        }
    }
}
