using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;

namespace Assets.Core.Entities
{
    [FucineImportable("dicta")]
    public class Dictum: AbstractEntity<Dictum>
    {
        [FucineValue]
        public string DefaultVerbSpecies { get; set; }

        public Dictum(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
            
        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
            //
        }
    }
}
