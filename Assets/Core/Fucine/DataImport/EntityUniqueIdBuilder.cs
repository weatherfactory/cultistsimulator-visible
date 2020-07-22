using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Assets.Core.Fucine.DataImport
{
    public class EntityUniqueIdBuilder
    {
        private StringBuilder uniqueId;

        public string BuiltId => uniqueId.ToString();


        public EntityUniqueIdBuilder(string startingId)
        {
            uniqueId = new StringBuilder(startingId);
        }

        public EntityUniqueIdBuilder(EntityUniqueIdBuilder basedOn)
        {
            uniqueId = new StringBuilder(basedOn.BuiltId);
        }





        public void WithObjectProperty(string containerId, string itemId)
        {
            uniqueId = uniqueId.Append($"::{containerId}[{itemId}]");
        }



        public void WithArray(string arrayId)
        {
            uniqueId.Append($">{arrayId}>");
        }


        public void WithLeaf(string propertyId)
        {
            uniqueId.Append("." + propertyId);
        }

    }
}
