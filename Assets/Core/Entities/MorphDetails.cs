using System;
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


    public class MorphDetails : IEntity
    {

        [FucineId]
        public string Id { get; set; }

        [FucineValue(100)]
        public int Chance { get; private set; }

        [FucineValue(1)]
        public int Level { get; private set; }

        [FucineValue(1)]
        public MorphEffectType MorphEffect { get; private set; }

        public MorphDetails()
        {}

        public MorphDetails(string id)
        {
            Id = id;
            Chance = 100;
            MorphEffect = MorphEffectType.Transform;
            Level = 1;

        }

        public MorphDetails(string id, int chance, MorphEffectType morphEffect, int level)
        {
            Id = id;
            Chance = chance;
            MorphEffect = morphEffect;
            Level = level;

        }
    }

}
