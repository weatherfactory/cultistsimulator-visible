using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;

namespace Assets.CS.TabletopUI.Interfaces
{
    public interface ITokenSubscriber
    {
        void TokenEffectCommandSent(DraggableToken draggableToken,EffectCommand effectCommand);
        void TokenPickedUp(DraggableToken draggableToken);
        void TokenInteracted(DraggableToken draggableToken);
    }
}
