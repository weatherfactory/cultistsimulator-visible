using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;

namespace Assets.Scripts.Entities
{
   public class AngelSpecification: AbstractEntity<AngelSpecification>
    {
        [FucineValue]
        public string Choir { get; set; }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
        }

        public AngelSpecification()
        {}

        public AngelSpecification(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

    }
}
