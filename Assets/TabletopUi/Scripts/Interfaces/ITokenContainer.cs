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
        bool AllowDrag { get; }
        /// <summary>
        /// use to manipulate elementstacks in the context where they're IElementStack
        /// </summary>
        /// <returns></returns>
        ElementStacksManager GetElementStacksManager();

        /// <summary>
        /// use to manipulate any token, including elementstacks seen as tokens
        /// </summary>
        /// <returns></returns>
        ITokenTransformWrapper GetTokenTransformWrapper();
    }
}
