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
        private StringBuilder nextKey;

        public EntityUniqueIdBuilder(string currentKey,string id)
        {
            nextKey=new StringBuilder($"{currentKey}|{id?.ToLower()}");
        }

        public string Key => nextKey.ToString();


        public void WithObjectProperty(string key)
        {
            nextKey = nextKey.Append($"{nextKey}{{{key}");
        }



        public void WithArray()
        {
            nextKey.Append("[");
        }


        public void WithLeaf(string propertyId)
        {
            nextKey.Append("|" + propertyId);
        }

    }
}
