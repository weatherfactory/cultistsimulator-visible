using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;

namespace Assets.Core.Entities
{
    [FucineImportable("histories")]
    public class History:AbstractEntity<History>
    {
        [FucineValue]
        public string DefaultCultureId { get; set; }

        public string CurrentCultureId { get; set; }

        [FucineValue]
        public float TimerBase { get; set; }


        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
            
        }

        public History(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

    }
}
