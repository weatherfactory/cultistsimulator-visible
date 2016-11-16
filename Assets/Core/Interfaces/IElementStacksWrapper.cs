using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Interfaces
{
    /// <summary>
    /// adds element stacks to a specified container
    /// </summary>
    public interface IElementStacksWrapper
    {
        IElementStack ProvisionElementStack(string elementId, int quantity);
        void Accept(IElementStack stack);
        IEnumerable<IElementStack> GetStacks();
    }
}
