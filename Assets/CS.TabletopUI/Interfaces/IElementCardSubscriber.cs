using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.CS.TabletopUI.Interfaces
{
    public interface IElementCardSubscriber
    {
        void ElementPickedUp(ElementCard elementCard);
    }
}
