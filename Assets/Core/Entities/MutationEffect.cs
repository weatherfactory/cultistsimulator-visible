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
    public class MutationEffect: IEntity
    {
        private readonly Hashtable _unknownProperties = CollectionsUtil.CreateCaseInsensitiveHashtable();

        [FucineValue("")]
        public string Filter { get; set; }

        [FucineValue("")]
        public string Mutate { get; set; }

        [FucineValue(0)]
        public int Level { get; set; }

        [FucineValue(false)]
        public bool Additive { get; set; }

        public MutationEffect()
        {
        }

        public void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {
            Hashtable unknownProperties = PopAllUnknownProperties();
            if (unknownProperties.Keys.Count > 0)
            {
                foreach (var k in unknownProperties.Keys)
                    logger.LogInfo($"Unknown property in import: {k} for MutationEffect (filter:{Filter}, mutate:{Mutate}, additive:{Additive})");
            }
        }

        public void PushUnknownProperty(object key, object value)
        {
            _unknownProperties.Add(key, value);
        }

        public Hashtable PopAllUnknownProperties()
        {
            Hashtable propertiesPopped = CollectionsUtil.CreateCaseInsensitiveHashtable(_unknownProperties);
            _unknownProperties.Clear();
            return propertiesPopped;
        }
    }
}
