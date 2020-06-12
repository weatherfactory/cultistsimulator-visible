﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    [FucineImport("verbs")]
    public class BasicVerb: IVerb,IEntity
    {
        [FucineId]
        public string Id { get; set; }

        [FucineString]
        public string Label { get; set; }

        [FucineString]
        public string Description { get; set; }

        [FucineEmanationProperty(typeof(SlotSpecification))]
        public SlotSpecification Slot { get; set; }


        public  bool Transient
        {
            get { return false; }
        }

        public BasicVerb(string id, string label, string description)
        {
            Id = id;
            Label = label;
            Description = description;
        }

        public BasicVerb()
        {

        }
    }
}
