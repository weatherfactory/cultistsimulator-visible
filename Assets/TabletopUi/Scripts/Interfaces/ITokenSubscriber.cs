using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Interfaces;

namespace Assets.CS.TabletopUI.Interfaces
{
    public interface ITokenSubscriber
    {
        void TokenEffectCommandSent(DraggableToken draggableToken,IEffectCommand effectCommand);
        void TokenPickedUp(DraggableToken draggableToken);
        void TokenInteracted(DraggableToken draggableToken);
    }
}
