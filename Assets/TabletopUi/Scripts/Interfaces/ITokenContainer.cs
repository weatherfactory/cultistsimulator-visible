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
        void TokenDropped(DraggableToken draggableToken);
        /// <summary>
        /// Called when an occupant has something else try to take its place. Should effect any changes necessary on the incumbent
        /// </summary>
        /// <param name="potentialUsurper"></param>
        /// <param name="IncumbentShouldMove"></param>
        void TryMoveAsideFor(DraggableToken potentialUsurper, DraggableToken incumbent, out bool incumbentShouldMove);
        bool AllowDrag { get; }
        bool AllowStackMerge { get; } //allow a stack dropped on a stack here to combine with it

        /// <summary>
        /// use to manipulate elementstacks in the context where they're IElementStack
        /// </summary>
        /// <returns></returns>
        ElementStacksManager GetElementStacksManager();

        string GetSaveLocationInfoForDraggable(DraggableToken draggable);

    }
}
