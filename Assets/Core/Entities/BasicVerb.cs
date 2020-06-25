﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    [FucineImportable("verbs")]
    public class BasicVerb: AbstractEntity,IVerb, IEntityWithId
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



        [FucineValue(".")]
        public string Label { get; set; }

        [FucineValue(".")]
        public string Description { get; set; }

        [FucineSubEntity(typeof(SlotSpecification))]
        public SlotSpecification Slot { get; set; }


        public  bool Transient
        {
            get { return false; }
        }

        public BasicVerb(string id, string label, string description)
        {
            _id = id;
            Label = label;
            Description = description;
        }

        public BasicVerb()
        {

        }

        public override HashSet<CachedFucineProperty> GetFucinePropertiesCached()
        {
            return TypeInfoCache<BasicVerb>.GetCachedFucinePropertiesForType();
        }
    }
}
