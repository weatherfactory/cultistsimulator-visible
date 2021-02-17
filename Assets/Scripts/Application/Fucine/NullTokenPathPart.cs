using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;

namespace Assets.Scripts.Application.Fucine
{
    public class NullTokenPathPart: TokenPathPart
    {
        public NullTokenPathPart() : base(string.Empty)
        {
        }

        public override PathCategory Category => PathCategory.Null;
    }
}
