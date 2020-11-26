using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;

namespace Assets.Core.Entities
{
   public class SphereSpecification:AbstractEntity<SphereSpecification>
    {

        [FucineValue (DefaultValue = "")]
        public string Label { get; set; }


        [FucineValue (DefaultValue = "")]
        public string Description { get; set; }

        [FucineValue]
        public string Category { get; set; }

        public SphereSpecification(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            
        }
    }
}
