using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Spheres;

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
            Thresholds=new List<SphereSpec>();

        }

        public  bool Transient => true;

        public string Art=>String.Empty;


        public int Quantity => 0;

        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
          return  typeof(VerbManifestation);
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            manifestation.InitialiseVisuals(this);
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public bool IsValidVerb()
        {
            return true;
        }


        public event Action OnChanged;
        public string Id { get; private set; }

        public void SetId(string id)
        {
            Id = id;
        }

        public string Label { get; set; }

        public string Description { get; set; }

        public List<SphereSpec> Thresholds { get; set; }

        public bool Startable { get; set; }
        public bool ExclusiveOpen => true;

        public string UniquenessGroup => string.Empty;
        public bool Unique => false;
        public bool Decays => false;

        public IAspectsDictionary GetAspects(bool includeSelf)
        {
            return new AspectsDictionary();
        }

        public void Decay(float interval)
        {
            //
        }

        public bool CanMergeWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public void SetQuantity(int quantityToLeaveBehind, Context context)
        {
            //
        }


        public bool Retire(RetirementVFX vfx)
        {
            return false;
        }

        public void AcceptIncomingPayloadForMerge(ITokenPayload incomingTokenPayload)
        {
            //
        }

        public void ShowNoMergeMessage(ITokenPayload incomingTokenPayload)
        {
            //
        }

        public void ModifyQuantity(int unsatisfiedChange, Context context)
        {
            //
        }

        public void ExecuteTokenEffectCommand(ITokenEffectCommand command)
        {
          //
        }
    }
}
