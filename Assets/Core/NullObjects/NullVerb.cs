using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;

namespace Assets.Core.NullObjects
{
    public class NullVerb:IVerb
    {
        public string Id { get; }
        public string Label { get; }
        public string Description { get; }
        public string Species { get; }
        public bool Transient { get; }
        public string Art => string.Empty;
        public SlotSpecification Slot { get; set; }
        public List<SlotSpecification> Slots { get; set; }
        public bool Startable { get; }

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
