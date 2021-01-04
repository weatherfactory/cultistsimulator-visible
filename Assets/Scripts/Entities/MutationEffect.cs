using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.Interfaces;

namespace SecretHistories.Entities
{
    public class MutationEffect: AbstractEntity<MutationEffect>,IEntityWithId
    {

        [FucineValue("")]
        public string Filter { get; set; }

        [FucineValue(ValidateAsElementId=true,DefaultValue="")]
        public string Mutate { get; set; }

        [FucineValue(0)]
        public int Level { get; set; }

        [FucineValue(false)]
        public bool Additive { get; set; }

        public MutationEffect(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
        }


        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            Hashtable unknownProperties = PopAllUnknownProperties();
            if (unknownProperties.Keys.Count > 0)
            {
                foreach (var k in unknownProperties.Keys)
                    log.LogInfo($"Unknown property in import: {k} for MutationEffect (filter:{Filter}, mutate:{Mutate}, additive:{Additive})");
            }
        }
    }
}
