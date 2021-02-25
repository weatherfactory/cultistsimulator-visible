using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;

namespace SecretHistories.Fucine
{
    public class RootPathPart: FucinePathPart
    {
        public RootPathPart() : base(FucinePath.ROOT.ToString())
        {
            
        }

        public override PathCategory Category => PathCategory.Root;
        public override string GetId()
        {
            return FucinePath.ROOT.ToString();
        }
    }
}
