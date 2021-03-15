using System;
using System.Collections.Generic;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.UI;

namespace SecretHistories.Entities
{
    [FucineImportable("verbs")]
    public class Verb: AbstractEntity<Verb>
    {
        public override string Id => _id;

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Label { get; set; }

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Description { get; set; }

        [FucineValue]
        public string Icon { get; set; }

        public bool Spontaneous { get; set; }

#pragma warning disable 67
        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
#pragma warning restore 67
        [Encaust]
        public int Quantity => 1;
        [Encaust]
        public Dictionary<string, int> Mutations { get; }



        public List<SphereSpec> Thresholds { get; set; } = new List<SphereSpec>();


        [FucineSubEntity(typeof(SphereSpec),Localise = true)]
        public SphereSpec Slot { get; set; }

        [FucineList(Localise = true)]
        public List<SphereSpec> Slots { get; set; }

        public virtual bool IsValid()
        {
            return true;
        }


        protected Verb(string id, string label, string description)
        {
            _id = id;
            Label = label;
            Description = description;
        }

        public static Verb CreateSpontaneousVerb(string id, string label, string description)
        {
            var v=new  Verb(id, label, description);
            v.Spontaneous = true;
            return v;
        }



        public string DefaultUniqueTokenId()
        {
            int identity = FucineRoot.Get().IncrementedIdentity();
            return $"{Id}_{identity}";
        }


        public Verb()
        {

        }

        public Verb(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            if (Slot != null)
                    Thresholds.Add(Slot); //what if this is empty? likely source of trouble later
                Thresholds.AddRange(Slots);
        }

    }
}
