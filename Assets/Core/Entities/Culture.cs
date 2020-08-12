using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    [FucineImportable("cultures")]
    public class Culture : AbstractEntity<Culture>
    {
        [FucineValue]
        public string Endonym { get; set; }

        [FucineValue]
        public string Exonym { get; set; }


        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
            //
        }

        public Culture(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

    }
}
