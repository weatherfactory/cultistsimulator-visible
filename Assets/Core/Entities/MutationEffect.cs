using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;

namespace Assets.Core.Entities
{
    public class MutationEffect
    {
        [FucineValue("")]
        public string Filter { get; set; }

        [FucineValue("")]
        public string Mutate { get; set; }

        [FucineValue(0)]
        public int Level { get; set; }

        [FucineValue(false)]
        public bool Additive { get; set; }

        public MutationEffect()
        {
        }
    }
}
