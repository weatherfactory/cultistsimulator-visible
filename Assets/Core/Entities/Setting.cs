using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using Noon;

namespace Assets.Core.Entities
{
    [FucineImportable("settings")]
    public class Setting : AbstractEntity<Setting>
        { 
        
            [FucineValue]
        public string Tab { get; set; }

        [FucineValue]
        public string Hint { get; set; }

        [FucineValue]
        public int MinValue { get; set; }
            [FucineValue]

        public int MaxValue { get; set; }

        [FucineDict]
        public Dictionary<string,string> ValueLabels { get; set; }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
            NoonUtility.Log("!");
           //do nowt
        }


        public Setting(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }
        }


}
