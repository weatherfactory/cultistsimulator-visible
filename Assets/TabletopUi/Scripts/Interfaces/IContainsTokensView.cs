using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

namespace Assets.CS.TabletopUI.Interfaces
{
    //strategy pattern to make TokenContainers (transformwrappers in Unity implementation) behave differently
    //This is a firmly Unity-level implementation at the moment, but we could tease out the concrete Token classes into more general interfaces if we needed to
    public interface IContainsTokensView {

        // Allow tokens to be dragged from here, or merged here
        bool AllowDrag { get; }
        bool AllowStackMerge { get; }

        ElementStacksManager GetElementStacksManager();

        IElementStack ProvisionElementStack(string elementId, int quantity, Source stackSource, string locatorId = null);

        void DisplayHere(IElementStack stack);
        void DisplayHere(DraggableToken token);

        void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved);
        void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved);

        void SignalElementStackRemovedFromContainer(ElementStackToken elementStackToken);

        string GetSaveLocationInfoForDraggable(DraggableToken draggable);

    }
}
