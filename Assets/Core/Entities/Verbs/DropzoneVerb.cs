using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts.Elements.Manifestations;

namespace Assets.Core.Entities.Verbs
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

        [FucineSubEntity(typeof(SlotSpecification), Localise = true)]
        public SlotSpecification Slot { get; set; }

        [FucineList(Localise = true)]
        public List<SlotSpecification> Slots { get; set; }

        public DropzoneVerb(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
           
        }


        public bool Transient => false;
        public Type AnchorManifestationType => typeof(DropzoneManifestation);
        public bool Startable => false;
        public bool ExclusiveOpen => false;
        public bool CreationAllowedWhenAlreadyExists(Situation s)
        {
            if (s.Verb.Id == this.Id)
                return false;
            return true;
        }
    }
}
