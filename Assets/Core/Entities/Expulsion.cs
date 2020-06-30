using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    public class Expulsion: AbstractEntity<Expulsion>,IEntityWithId
    {
        public string Id { get; private set; }

        public void SetId(string id)
        {
            Id = id;
        }


        [FucineAspects]
        public AspectsDictionary Filter { get; set; }
        
        [FucineValue(1)] 
        public int Limit { get; set; }

        public Expulsion(Hashtable importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
            Filter = new AspectsDictionary();
        }

        public Expulsion()
        {}


    }
}