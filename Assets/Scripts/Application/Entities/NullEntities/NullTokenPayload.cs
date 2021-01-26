using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Enums;

namespace Assets.Scripts.Application.Entities.NullEntities
{
    public class NullTokenPayload: ITokenPayload
    {
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

    }
}
