using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;

namespace Assets.Core.Interfaces
{
    public interface ElementStackTokensManager
    {
        

        int GetCurrentElementQuantity(string elementId);
        IDictionary<string, int> GetCurrentElementTotals();
        AspectsDictionary GetTotalAspects(bool showElementAspects = true);
        IEnumerable<ElementStackToken> GetStacks();

        bool PersistBetweenScenes { get; }

        void AcceptStack(ElementStackToken stack, Context context);
        void AcceptStacks(IEnumerable<ElementStackToken> stacks, Context context);
        void RemoveStack(ElementStackToken stack);

        void ModifyElementQuantity(string elementId, int quantityChange, Source stackSource, Context context);

        // for debugging reference
        string Name { get; set; }

        void Deregister();
        void RemoveAllStacks();
     
        int IncreaseElement(string elementId, int quantityChange, Source stackSource, Context context, string locatorId = null);
        int ReduceElement(string elementId, int quantityChange, Context context);
        int PurgeElement(Element element, int maxToPurge);
        ElementStackToken AddAndReturnStack(string elementId, int quantity, Source stackSource, Context context);
        void NotifyStacksChanged();
    }
}
