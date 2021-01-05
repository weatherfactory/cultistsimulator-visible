using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Interfaces;

namespace SecretHistories.NullObjects
{
    public class NullSituation: Situation
    {
        public override IVerb Verb { get; set; }=NullVerb.Create();
        

        public override void OpenAtCurrentLocation()
        {
            //
        }

        public override bool IsValidSituation()
        {
            return false;
        }

    }
}
