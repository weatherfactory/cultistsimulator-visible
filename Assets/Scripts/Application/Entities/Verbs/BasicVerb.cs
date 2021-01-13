using System;
using System.Collections.Generic;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Elements.Manifestations;

namespace SecretHistories.Entities
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

        public Type GetDefaultManifestationType()
        {
            return typeof(VerbManifestation);
        }

        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
            return typeof(VerbManifestation);
        }

        [FucineSubEntity(typeof(SphereSpec),Localise = true)]
        public SphereSpec Slot { get; set; }

        [FucineList(Localise = true)]
        public List<SphereSpec> Slots { get; set; }



        public bool Transient
        {
            get { return false; }
        }
        
        [FucineValue(DefaultValue = true)]
        public bool Startable { get; set; }

        public bool ExclusiveOpen => true;


        public Situation CreateDefaultSituation(TokenLocation anchorLocation)
        {
            throw new NotImplementedException();
        }

        
        
        
        public BasicVerb(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {

        }
    }
}
