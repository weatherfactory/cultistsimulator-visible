using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;

namespace SecretHistories.Elements

{
    public class NullSituationPath: SituationPath
    {

        public NullSituationPath() : base(string.Empty)
        {

        }

        public override SituationPath GetBaseSituationPath()
        {
            return this;
        }

    }
}
