using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    public class BasicVerb: AbstractVerb, IVerb
    {

        public override bool Transient
        {
            get { return false; }
        }
    }
}
