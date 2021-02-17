using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using JetBrains.Annotations;

namespace SecretHistories.Entities
{

    public class MorphDetails : AbstractEntity<MorphDetails>, IEntityWithId,IQuickSpecEntity
    {


        [FucineValue(100)]
        public int Chance { get; private set; }

        [FucineValue(1)]
        public int Level { get; private set; }

        [FucineValue((int)MorphEffectType.Transform)]
        public MorphEffectType MorphEffect { get; private set; }

        public MorphDetails(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        { }

        public MorphDetails(string id)
        {
            _id = id;
            Chance = 100;
            MorphEffect = MorphEffectType.Transform;
            Level = 1;

        }

        public MorphDetails(string id, int chance, MorphEffectType morphEffect, int level)
        {
            _id = id;
            Chance = chance;
            MorphEffect = morphEffect;
            Level = level;

        }

        public void QuickSpec(string value)
        {
            _id = value;
            Chance = 100;
            MorphEffect = MorphEffectType.Transform;
            Level = 1;

        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            Hashtable unknownProperties = PopAllUnknownProperties();
            if (unknownProperties.Keys.Count > 0)
            {
                foreach (var k in unknownProperties.Keys)

                    log.LogInfo($"Unknown property in import: {k} for MorphDetails ({Id} {Chance} {MorphEffect} {Level})");

            }
        }

        public MorphDetails()
        {

        }

    }



}
