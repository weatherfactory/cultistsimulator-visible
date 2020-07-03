using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assets.Core.Fucine.DataImport
{
   public class EntityData
    {
        public Hashtable Data { get; set; }
        public Hashtable LocalisedData { get; set; }

        public EntityData()
        {
            Data = new Hashtable();
        }

        public EntityData(Hashtable data)
        {
            Data = data;
        }


        public EntityData(Hashtable data, Hashtable localisedData)
        {
            Data = data;
            LocalisedData = localisedData;
        }
    }
}
