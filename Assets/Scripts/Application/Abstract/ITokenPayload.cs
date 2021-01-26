using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Enums;

namespace SecretHistories.Abstract
    {
    public interface ITokenPayload: IEncaustable
    {
        public string Id { get; }
        public int Quantity { get; }
        Type GetManifestationType(SphereCategory sphereCategory);
        void InitialiseManifestation(IManifestation manifestation);
        bool IsValidElementStack();
        bool IsValidVerb();
        public string Label { get; }
        public string Description { get; }
    }
    }

