using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Enums;

namespace SecretHistories.Abstract
    {
    public interface ITokenPayload
    {
        public string Id { get; }
        Type GetManifestationType(SphereCategory sphereCategory);
        void InitialiseManifestation(IManifestation manifestation);
    }
    }

