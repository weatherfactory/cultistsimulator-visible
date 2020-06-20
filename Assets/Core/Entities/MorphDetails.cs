﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using JetBrains.Annotations;

namespace Assets.Core.Entities
{


    public class MorphDetails : IEntityWithId,IQuickSpecEntity
    {

        private string _id;

        [FucineId]
        public string Id
        {
            get => _id;
        }

        public void SetId(string id)
        {
            _id = id;
        }


        [FucineValue(100)]
        public int Chance { get; private set; }

        [FucineValue(1)]
        public int Level { get; private set; }

        [FucineValue((int)MorphEffectType.Transform)]
        public MorphEffectType MorphEffect { get; private set; }

        public MorphDetails()
        {}

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

        public void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {

        }
    }



}
