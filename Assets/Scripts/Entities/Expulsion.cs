using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.Interfaces;

namespace SecretHistories.Entities
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


        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            
        }
    }
}