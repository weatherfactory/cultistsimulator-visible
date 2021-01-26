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


        public int Quantity => 1;

        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
            return typeof(VerbManifestation);
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            manifestation.InitialiseVisuals(this);
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public bool IsValidVerb()
        {
            return true;
        }

        public List<SphereSpec> Thresholds
        {
            get
            {
                var aggregatedSlotsFromOldAndNewFormat = new List<SphereSpec>();
                aggregatedSlotsFromOldAndNewFormat.Add(Slot); //what if this is empty? likely source of trouble later
                aggregatedSlotsFromOldAndNewFormat.AddRange(Slots);
                return aggregatedSlotsFromOldAndNewFormat;
            }


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

        public BasicVerb(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {

        }
    }
}
