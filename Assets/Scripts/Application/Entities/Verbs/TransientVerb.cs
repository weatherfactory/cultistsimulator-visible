using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Elements.Manifestations;
using SecretHistories.TokenContainers;

namespace SecretHistories.Entities
{
   public class TransientVerb: IVerb
    {
        public TransientVerb()
        {
            Startable = false;
        }

        public TransientVerb(string id, string label, string description):this()
        {
            Id = id;
            Label = label;
            Description = description;
            Slots=new List<SlotSpecification>();

        }

        public  bool Transient => true;

        public string Art=>String.Empty;
        public Type GetDefaultManifestationType()
        {
            return typeof(VerbManifestation);

        }

        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
          return  typeof(VerbManifestation);
        }

        

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
        public bool ExclusiveOpen => true;

        public Situation CreateDefaultSituation(TokenLocation anchorLocation)
        {
            throw new NotImplementedException();
        }


        public bool AllowMultipleInstances => false;
    }
}
