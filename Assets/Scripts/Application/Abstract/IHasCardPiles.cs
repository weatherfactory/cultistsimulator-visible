using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Spheres;

namespace SecretHistories.Abstract
{
    public interface IHasCardPiles
    {
        IEnumerable<IHasElementTokens> GetDrawPiles();
        IHasElementTokens GetDrawPile(string forDeckSpecId);
        IHasElementTokens GetForbiddenPile(string forDeckSpecId);
    }
}
