using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    public class MutationEffect: AbstractEntity<MutationEffect>,IEntityWithId
    {
        public string Id { get; private set; }

        public void SetId(string id)
        {
            Id = id;
        }


        [FucineValue("")]
        public string Filter { get; set; }

        [FucineValue("")]
        public string Mutate { get; set; }

        [FucineValue(0)]
        public int Level { get; set; }

        [FucineValue(false)]
        public bool Additive { get; set; }

        public MutationEffect(Hashtable importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
        }

        public override void RefineWithCompendium(ContentImportLog log, ICompendium populatedCompendium)
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
