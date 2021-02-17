using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;

namespace Assets.Scripts.Application.Fucine
{
  public  class NullFucinePath: FucinePath
    {
        public NullFucinePath() : base(string.Empty)
        {
        }
        
        public override bool IsValid()
        {
            return false;
        }
    }
}
