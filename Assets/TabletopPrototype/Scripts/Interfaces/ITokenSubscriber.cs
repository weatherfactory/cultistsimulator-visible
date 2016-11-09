using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.CS.TabletopUI.Interfaces
{
    public interface ITokenSubscriber
    {
        void TokenPickedUp(Draggable draggable);
    }
}
