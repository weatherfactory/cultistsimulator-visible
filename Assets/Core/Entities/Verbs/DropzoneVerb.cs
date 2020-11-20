using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
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

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
            throw new NotImplementedException();
        }


        public bool Transient => false;
        public Type AnchorManifestationType => typeof(DropzoneManifestation);
        public bool Startable => false;
        public bool ExclusiveOpen => false;
        public bool CreationAllowedWhenAlreadyExists(Situation s)
        {
            return false;
        }
    }
}
