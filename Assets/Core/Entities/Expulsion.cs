using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    public class Expulsion: AbstractEntity<Expulsion>,IEntityWithId
    {

        [FucineAspects(ValidateAsElementId = true)]
        public AspectsDictionary Filter { get; set; }
        
        [FucineValue(1)] 
        public int Limit { get; set; }

        public Expulsion(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
   
        }

        public Expulsion()
        {

        }


        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
            
        }
    }
}