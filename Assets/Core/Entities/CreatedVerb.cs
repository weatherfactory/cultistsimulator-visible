using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
   public class CreatedVerb: AbstractVerb
    {
        public CreatedVerb(string id, string label, string description) : base(id, label, description)
        {

        }

        public override bool Transient
        {
            get { return true; }
        }
    }
}
