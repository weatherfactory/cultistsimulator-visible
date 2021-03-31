using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;

namespace SecretHistories.Abstract
{
    public interface IHasElementTokens
    {
        string GetDeckSpecId();
        List<Token> GetElementTokens();
        int GetTotalStacksCount();
        void RetireTokensWhere(Func<Token, bool> filter);
        void AcceptToken(Token t, Context c);
        Token ProvisionElementToken(string card, int i);
    }
}
