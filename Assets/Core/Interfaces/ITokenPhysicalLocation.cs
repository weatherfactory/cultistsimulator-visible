using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;

namespace Assets.Core.Interfaces
{
    /// <summary>
    /// adds element stacks to a specified containsTokens
    /// </summary>
    public interface ITokenPhysicalLocation
    {
        IElementStack ProvisionElementStack(string elementId, int quantity,Source stackSource, string locatorId=null);
        void DisplayHere(IElementStack stack);
        void DisplayHere(DraggableToken token);
        IEnumerable<IElementStack> GetStacks();
        IEnumerable<SituationToken> GetSituationTokens();
        IEnumerable<DraggableToken> GetTokens();
        ElementStackToken ProvisionElementStackAsToken(string elementId, int quantity, Source stackSource, string locatorid = null);

    }
}
