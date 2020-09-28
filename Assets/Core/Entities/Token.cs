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
    
    [FucineImportable("tokens")]
    public class Token : AbstractEntity<Token>
    {
        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
       
        }

        public Token(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }
    }
}
