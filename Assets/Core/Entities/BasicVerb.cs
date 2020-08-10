using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    [FucineImportable("verbs")]
    public class BasicVerb: AbstractEntity<BasicVerb>,IVerb, IEntityWithId
    {

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Label { get; set; }

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Description { get; set; }

        [FucineSubEntity(typeof(SlotSpecification),Localise = true)]
        public SlotSpecification Slot { get; set; }


        public  bool Transient
        {
            get { return false; }
        }

        //public BasicVerb(string id, string label, string description)
        //{
        //    _id = id;
        //    Label = label;
        //    Description = description;
        //}

        public BasicVerb(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
        }
    }
}
