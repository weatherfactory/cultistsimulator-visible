using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;

namespace SecretHistories.Entities
{

    [FucineImportable("portals")]
    public class Portal: AbstractEntity<Portal>
    {
        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Label { get; set; }

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Description { get; set; }

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Icon { get; set; }

        [FucineList(Localise = true)]
        public List<LinkedRecipeDetails> Consequences { get; set; }


        public Portal(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
           //
        }
    }
}
