using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;

namespace Assets.Core.Interfaces
{
    public interface IElementStacksManager
    {
        

        int GetCurrentElementQuantity(string elementId);
        IDictionary<string, int> GetCurrentElementTotals();
        AspectsDictionary GetTotalAspects(bool showElementAspects = true);
        IEnumerable<IElementStack> GetStacks();

        void AcceptStack(IElementStack stack, Context context);
        void AcceptStacks(IEnumerable<IElementStack> stacks, Context context);
        void RemoveStack(IElementStack stack);

        void ModifyElementQuantity(string elementId, int quantityChange, Source stackSource, Context context);

        // for debugging reference
        string Name { get; set; }

        void Deregister();
        void RemoveAllStacks();
     
        int IncreaseElement(string elementId, int quantityChange, Source stackSource, Context context, string locatorId = null);
        int ReduceElement(string elementId, int quantityChange, Context context);
        int PurgeElement(Element element, int maxToPurge, Context context);
        IElementStack AddAndReturnStack(string elementId, int quantity, Source stackSource, Context context);
    }
}
