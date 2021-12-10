using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;

namespace Assets.Scripts.Application.Fucine
{
    public class WildPathPart: FucinePathPart
    {
        public WildPathPart() : base(FucinePath.WILD.ToString())
        {
        }

        public override PathCategory Category => PathCategory.Wild;
        public override string GetId()
        {
            return FucinePath.WILD.ToString();
        }
    }
}
