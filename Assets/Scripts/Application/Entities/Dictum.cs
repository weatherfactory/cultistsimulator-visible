using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;

namespace SecretHistories.Entities
{
    [FucineImportable("dicta")]
    public class Dictum: AbstractEntity<Dictum>
    {

        [FucineValue]
        public string DefaultWorldSpherePath { get; set; }


        [FucineValue]
        public string DefaultEnRouteSpherePath { get; set; }

        [FucineValue]
        public string DefaultWindowSpherePath { get; set; }


        public Dictum(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
            
        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            //
        }
    }
}
