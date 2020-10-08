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
    public interface ITokenContainer {

        // Allow tokens to be dragged from here, or merged here
        bool AllowDrag { get; }
        bool AllowStackMerge { get; }
        bool AlwaysShowHoverGlow { get;}
        bool PersistBetweenScenes { get; }

        ElementStackToken ProvisionElementStack(string elementId, int quantity, Source stackSource, Context context, string locatorId = null);

        void DisplayHere(ElementStackToken stack, Context context);
        void DisplayHere(IToken token, Context context);

        void TryMoveAsideFor(ElementStackToken potentialUsurper, AbstractToken incumbent, out bool incumbentMoved);
        void TryMoveAsideFor(VerbAnchor potentialUsurper, AbstractToken incumbent, out bool incumbentMoved);

        void SignalStackAdded(ElementStackToken elementStackToken, Context context);
        void SignalStackRemoved(ElementStackToken elementStackToken, Context context);

        string GetSaveLocationInfoForDraggable(AbstractToken @abstract);

        void ModifyElementQuantity(string elementId, int quantityChange, Source stackSource, Context context);

        /// <summary>
        /// Reduces matching stacks until change is satisfied
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="quantityChange">must be negative</param>
        /// <returns>returns any unsatisfied change remaining</returns>
        int ReduceElement(string elementId, int quantityChange, Context context);

        int IncreaseElement(string elementId, int quantityChange, Source stackSource, Context context, string locatorid = null);
        IEnumerable<ElementStackToken> GetStacks();

        /// <summary>
        /// All the aspects in all the stacks, summing the aspects
        /// </summary>
        /// <returns></returns>
        AspectsDictionary GetTotalAspects(bool includingSelf = true);

        int PurgeElement(Element element, int maxToPurge);
        void AcceptStack(ElementStackToken stack, Context context);
        void RemoveDuplicates(ElementStackToken incomingStack);
        void AcceptStacks(IEnumerable<ElementStackToken> stacks, Context context);

        /// <summary>
        /// removes the stack from this stack manager; doesn't retire the stack
        /// </summary>
        /// <param name="stack"></param>
        void RemoveStack(ElementStackToken stack);

        /// <summary>
        /// removes the stacks from this stack manager; doesn't retire the stack
        /// </summary>
        void RemoveAllStacks();

        void RetireAllStacks();
        void NotifyStacksChanged();

        /// <summary>
        /// This was relevant for a refactoring of the greedy slot code; I decided to do something else
        /// but this code might still be useful elsewhere!
        /// </summary>
        /// <param name="requirement"></param>
        /// <returns></returns>
        List<ElementStackToken> GetStacksWithAspect(KeyValuePair<string, int> requirement);
    }
}
