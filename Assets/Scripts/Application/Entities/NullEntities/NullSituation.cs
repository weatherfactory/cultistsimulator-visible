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
        private static NullSituation _instance;
 
        public override bool IsValidSituation()
        {
            return false;
        }

        protected NullSituation(Verb verb) : base(verb,verb.DefaultUniqueTokenId())
        {
        }

        public static NullSituation Create()
        {
            if(_instance==null)
                _instance= new NullSituation(NullVerb.Create());

            return _instance;
        }
    }
}
