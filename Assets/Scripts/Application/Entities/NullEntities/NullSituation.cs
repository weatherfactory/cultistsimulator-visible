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

        public NullSituation(FucinePath path,Verb verb) : base(path,verb)
        {
        }

        public static NullSituation Create()
        {
            return new NullSituation(FucinePath.Root(),NullVerb.Create());
        }
    }
}
