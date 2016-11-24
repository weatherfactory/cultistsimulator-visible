using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Interfaces;

namespace Assets.CS.TabletopUI.Interfaces
{
    public interface ITokenContainer
    {
        void TokenPickedUp(DraggableToken draggableToken);
        void TokenInteracted(DraggableToken draggableToken);
        bool AllowDrag { get; }
        ElementStacksManager GetElementStacksManager();
    }
}
