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
    [FucineImportable("dropzones")]
    public class DropzoneVerb: AbstractEntity<DropzoneVerb>,IVerb
    {
        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Label { get; set; }

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Description { get; set; }

        [FucineValue]
        public string Art { get; set; }

        public Type GetDefaultManifestationType()
        {
            return typeof(DropzoneManifestation);
        }

        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
            return typeof(DropzoneManifestation);
        }

        [FucineSubEntity(typeof(SphereSpec), Localise = true)]
        public SphereSpec Slot { get; set; }

        [FucineList(Localise = true)]
        public List<SphereSpec> Slots { get; set; }

        public DropzoneVerb(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
       
        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
           
        }

        public bool Transient => false;
        public bool Startable => false;
        public bool ExclusiveOpen => false;

        public Situation CreateDefaultSituation(TokenLocation anchorLocation)
        {
            var dropzoneRecipe = Registry.Get<Compendium>().GetEntityById<Recipe>(NoonConstants.DROPZONE_RECIPE_ID);

            var cmd = new SituationCreationCommand(this, dropzoneRecipe, StateEnum.Unstarted, anchorLocation,
                null);
           var dropzoneSituation= Registry.Get<SituationBuilder>().CreateSituationWithAnchorAndWindow(cmd);
           
           return dropzoneSituation;
        }
    }
}
