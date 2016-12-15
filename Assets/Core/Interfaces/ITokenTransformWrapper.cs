using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;

namespace Assets.Core.Interfaces
{
    /// <summary>
    /// adds element stacks to a specified container
    /// </summary>
    public interface ITokenTransformWrapper
    {
        IElementStack ProvisionElementStack(string elementId, int quantity,string locatorId=null);
        void Accept(IElementStack stack);
        void Accept(DraggableToken token);
        IEnumerable<IElementStack> GetStacks();
        IEnumerable<SituationToken> GetSituationTokens();
        IEnumerable<DraggableToken> GetTokens();
        ElementStackToken ProvisionElementStackAsToken(string elementId, int quantity, string locatorid = null);
    }
}
