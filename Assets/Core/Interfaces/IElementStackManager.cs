using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;

namespace Assets.Core.Interfaces
{
    public interface IElementStacksManager
    {
        /// <summary>
        /// Reduces matching stacks until change is satisfied - NB a match is also a stack which possesses this aspect
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="quantityChange">must be negative</param>
        /// <returns>returns any unsatisfied change remaining</returns>
        int ReduceElement(string elementId, int quantityChange, Context context);
        int IncreaseElement(string elementId, int quantityChange, Source stackSource, Context context, string locatorId = null);

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
    }
}
