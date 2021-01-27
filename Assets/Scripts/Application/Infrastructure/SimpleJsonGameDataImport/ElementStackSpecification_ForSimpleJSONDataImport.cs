using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;

namespace Assets.Scripts.Application.Infrastructure.SimpleJsonGameDataImport
{
    public class ElementStackSpecification_ForSimpleJSONDataImport
    {
        /// <summary>
        /// The element id
        /// </summary>
        public string Id { get; set; }
        public int Quantity { get; set; }
        public string LocationInfo { get; set; }
        public Dictionary<string, int> Mutations { get; set; }
        public Dictionary<string, string> Illuminations { get; set; }

        public int Depth { get; set; }
        public float LifetimeRemaining { get; set; }
        public bool MarkedForConsumption { get; set; }
        public Context Context { get; set; }

        public ElementStackSpecification_ForSimpleJSONDataImport(string id, int quantity, string locationInfo, Dictionary<string, int> mutations, Dictionary<string, string> illuminations, float lifetimeRemaining,Context context)
        {
            Id = id;
            Quantity = quantity;
            LocationInfo = locationInfo;
            Depth = locationInfo.Count(c => c == SpherePath.SEPARATOR);
            Mutations = new Dictionary<string, int>(mutations);
            Illuminations = new Dictionary<string, string>(illuminations);
            LifetimeRemaining = lifetimeRemaining;
            Context = context;
        }

    }
}
