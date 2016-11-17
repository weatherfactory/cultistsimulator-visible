using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
   public class TransientVerb: Verb
    {
        public TransientVerb(string id, string label, string description) : base(id, label, description)
        {
        }
    }
}
