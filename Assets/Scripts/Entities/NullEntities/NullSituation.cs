using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

namespace Assets.Core.NullObjects
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
