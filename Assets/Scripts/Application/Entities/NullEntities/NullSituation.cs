using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;

namespace SecretHistories.NullObjects
{
    public class NullSituation: Situation
    {
        
 
        public override bool IsValidSituation()
        {
            return false;
        }

        public NullSituation(Verb verb) : base(verb)
        {
        }

        public static NullSituation Create()
        {
            return new NullSituation(NullVerb.Create());
        }
    }
}
