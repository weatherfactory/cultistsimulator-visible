using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;

namespace Assets.Scripts.Application.Fucine
{
    public class CurrentLocationPathPart: FucinePathPart
    {
        public CurrentLocationPathPart() : base(String.Empty)
        {
        }

        public override PathCategory Category => PathCategory.Current;
        public override string GetId()
        {
            return string.Empty;
        }
    }
}
