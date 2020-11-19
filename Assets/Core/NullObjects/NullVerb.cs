using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

namespace Assets.Core.NullObjects
{
    public class NullVerb:IVerb
    {
        public string Id { get; private set; }
        public void SetId(string id)
        {
            Id = id;
        }

        public string Label { get; set; }
        public string Description { get; set; }
        public string SpeciesId { get; }
        public Species Species { get; }
        public bool Transient { get; }
        public string Art => string.Empty;
        public SlotSpecification Slot { get; set; }
        public List<SlotSpecification> Slots { get; set; }
        public bool Startable { get; }
        public bool CreationAllowedWhenAlreadyExists(Situation s)
        {
            return true;
        }

        public bool AllowMultipleInstances => true;

        protected NullVerb()
        {
            Slots=new List<SlotSpecification>();
            Slot=new SlotSpecification();
            Startable = false;
        }


        public static NullVerb Create()
        {
            return new NullVerb();
        }
    }
}
