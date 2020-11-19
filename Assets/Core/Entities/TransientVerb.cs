using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;

namespace Assets.Core.Entities
{
   public class TransientVerb: IVerb
    {
        public TransientVerb()
        {
            Startable = false;
            SpeciesId = Registry.Get<ICompendium>().GetSingleEntity<Dictum>().DefaultTransientVerbSpecies;
            Species = Registry.Get<ICompendium>().GetEntityById<Species>(SpeciesId);
        }

        public TransientVerb(string id, string label, string description):this()
        {
            Id = id;
            Label = label;
            Description = description;

        }

        public string SpeciesId { get; private set; }
        public Species Species { get; private set; }

        public  bool Transient => true;

        public string Art=>String.Empty;


        public string Id { get; private set; }

        public void SetId(string id)
        {
            Id = id;
        }

        public string Label { get; set; }

        public string Description { get; set; }

        public SlotSpecification Slot { get; set; }
        public List<SlotSpecification> Slots { get; set; }
        public bool Startable { get; set; }
        public bool AllowMultipleInstances => false;
    }
}
