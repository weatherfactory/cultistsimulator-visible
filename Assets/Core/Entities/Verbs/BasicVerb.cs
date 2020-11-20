﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Elements.Manifestations;

namespace Assets.Core.Entities
{
    [FucineImportable("verbs")]
    public class BasicVerb: AbstractEntity<BasicVerb>,IVerb
    {

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Label { get; set; }

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Description { get; set; }

        
        [FucineValue]
        public string Art { get; set; }

        [FucineSubEntity(typeof(SlotSpecification),Localise = true)]
        public SlotSpecification Slot { get; set; }

        [FucineList(Localise = true)]
        public List<SlotSpecification> Slots { get; set; }


        public Type AnchorManifestationType => typeof(VerbManifestation);

        public bool Transient
        {
            get { return false; }
        }
        
        [FucineValue(DefaultValue = true)]
        public bool Startable { get; set; }

        public bool ExclusiveOpen => true;

        public bool CreationAllowedWhenAlreadyExists(Situation s)
        {
            if (s.Verb.Id == this.Id)
                return false;
            return true;

        }

        public BasicVerb(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {

        }
    }
}
