using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Enums;
using SecretHistories.Interfaces;

namespace Assets.Scripts.Application.Entities.NullEntities
{
    public class NullTokenPayload: ITokenPayload
    {
        public event Action OnChanged;
        public string Id => string.Empty;
        public int Quantity => 0;
        public string Label => "";
        public string Description => "";


        public Type GetManifestationType(SphereCategory sphereCategory)
        {
            return typeof(NullManifestation);
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            //
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public bool IsValidVerb()
        {
            return false;
        }
        public string UniquenessGroup => string.Empty;
        public bool Unique => false;
        public bool Decays => false;

        public IAspectsDictionary GetAspects(bool includeSelf)
        {
            return new AspectsDictionary();
        }


        public void Decay(float interval)
        {
            return;
        }

        public bool CanMergeWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public void SetQuantity(int quantityToLeaveBehind, Context context)
        {
            //
        }

        bool ITokenPayload.Retire(RetirementVFX vfx)
        {
            throw new NotImplementedException();
        }

        public void Retire(RetirementVFX vfx)
        {
            //
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
