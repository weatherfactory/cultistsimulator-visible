using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Interfaces
{
    /// <summary>
    /// adds element stacks to a specified container
    /// </summary>
    public interface IElementStackProvisioner
    {
        IElementStack ProvisionElementStack(string elementId, int quantity);
    }
}
