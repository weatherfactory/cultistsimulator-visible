using System;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Services;

namespace SecretHistories.Entities.Verbs
{
    public class DropzoneVerb: IVerb
    {
        public string Id { get; private set; }

        public void SetId(string id)
        {
            Id = id;
        }

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Label { get; set; }

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Description { get; set; }
         
        [FucineValue]
        public string Art { get; set; }


        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
            return typeof(DropzoneManifestation);
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            manifestation.InitialiseVisuals(this);
        }

        public List<SphereSpec> Thresholds { get; set; }
    


        public bool Transient => false;
        public bool Startable => false;
        public bool ExclusiveOpen => false;

        public DropzoneVerb()
        {
            Thresholds=new List<SphereSpec>();
        }

        public static DropzoneVerb Create()
        {
            return new DropzoneVerb();

        }
    }
}
