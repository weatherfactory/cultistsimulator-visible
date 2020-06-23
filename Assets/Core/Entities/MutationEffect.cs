using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    public class MutationEffect: IEntity
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

        public void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {
            
        }
    }
}
