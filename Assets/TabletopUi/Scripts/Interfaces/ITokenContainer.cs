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
        void ElementStackRemovedFromContainer(ElementStackToken elementStackToken);
        /// <summary>
        /// Called when an occupant has something else try to take its place. Should effect any changes necessary on the incumbent
        /// </summary>
        /// <param name="potentialUsurper"></param>
        /// <param name="IncumbentShouldMove"></param>
        void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved);
        void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved);
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
