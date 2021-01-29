using System;
using System.Collections.Generic;
using Assets.Scripts.Application.Infrastructure.Events;
using Assets.Scripts.Application.Logic;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Core;
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
        [Encaust]
        public override string Id => _id;

        [FucineValue(DefaultValue = ".", Localise = true)]
        [Encaust]
        public string Label { get; set; }

        [FucineValue(DefaultValue = ".", Localise = true)]
        [Encaust]
        public string Description { get; set; }

        [FucineValue]
        [Encaust]
        public string Art { get; set; }


        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
        [Encaust]
        public int Quantity => 1;
        [Encaust]
        public Dictionary<string, int> Mutations { get; }

        public Timeshadow GetTimeshadow()
        {
            return Timeshadow.CreateTimelessShadow();
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
        

        public bool ExclusiveOpen => true;

        public BasicVerb()
        {

        }

        public BasicVerb(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {

        }

    }
}
