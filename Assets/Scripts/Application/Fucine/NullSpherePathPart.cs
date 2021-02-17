using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Fucine;
using SecretHistories.Fucine;

namespace SecretHistories.Fucine
{
    public class NullSpherePathPart: SpherePathPart
    {
        public NullSpherePathPart() : base(string.Empty)
        {
        }

        public override PathCategory Category => PathCategory.Null;
    }
}
