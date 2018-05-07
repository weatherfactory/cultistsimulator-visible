using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    public class MutationEffect
    {
        public string FilterOnAspectId { get; set; }
        public string MutateAspectId { get; set; }
        public int MutationLevel { get; set; }
        public bool Additive { get; set; }

        public MutationEffect(string filterOnAspectId, string mutateAspectId, int mutationLevel, bool additive)
        {
            FilterOnAspectId = filterOnAspectId;
            MutateAspectId = mutateAspectId;
            MutationLevel = mutationLevel;
            Additive = additive;
        }
    }
}
