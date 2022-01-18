using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;

namespace SecretHistories.Assets.Scripts.Application.Entities
{
    [FucineImportable("rooms")]
    public class Room: AbstractEntity<Room>
    {
        [FucineValue]
        public string Label { get; set; }
        [FucineValue]
        public string Description { get; set; }
        public Room(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
         //
        }
    }
}
