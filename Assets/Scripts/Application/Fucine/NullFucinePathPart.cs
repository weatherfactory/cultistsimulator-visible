using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Fucine;
using SecretHistories.Fucine;

namespace SecretHistories.Fucine
{
    public class NullFucinePathPart: FucinePathPart
    {
        public NullFucinePathPart() : base(null)
        {
        }

        public override PathCategory Category => PathCategory.Null;
        public override string GetId()
        {
            return null;
        }
    }
}
