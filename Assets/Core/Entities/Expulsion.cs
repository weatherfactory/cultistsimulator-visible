using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    public class Expulsion: AbstractEntity
    {

        [FucineAspects]
        public AspectsDictionary Filter { get; set; }
        
        [FucineValue(1)] 
        public int Limit { get; set; }

        public Expulsion()
        {
            Filter = new AspectsDictionary();
        }

        public override HashSet<CachedFucineProperty> GetFucinePropertiesCached()
        {
            return TypeInfoCache<Expulsion>.GetCachedFucinePropertiesForType();
        }
    }
}