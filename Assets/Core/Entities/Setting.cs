using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    [FucineImportable("configcontrols")]
    public class ConfigControl : AbstractEntity<ConfigControl>
        {
            public string Tab { get; set; }
            public string Hint { get; set; }
            public string HintLocId { get; set; }
            public float MinValue { get; set; }
            public float MaxValue { get; set; }
            protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
            {
                throw new NotImplementedException();
            }
        }


}
