using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;

namespace Assets.Core.Entities
{
   public class CreatedVerb: IVerb
    {
        public CreatedVerb(string id, string label, string description)
        {
            Id = id;
            Label = label;
            Description = description;
            Startable = false;
            SpeciesId = Registry.Get<ICompendium>().GetSingleEntity<Dictum>().DefaultVerbSpecies;
            Species = Registry.Get<ICompendium>().GetEntityById<Species>(SpeciesId);
        }

        public string SpeciesId { get; private set; }
        public Species Species { get; private set; }

        public  bool Transient => true;

        public string Art=>String.Empty;


        public string Id { get; set; }

        public string Label { get; set; }

        public string Description { get; set; }

        public SlotSpecification Slot { get; set; }
        public List<SlotSpecification> Slots { get; set; }
        public bool Startable { get; set; }
        public bool AllowMultipleInstances => false;
    }
}
