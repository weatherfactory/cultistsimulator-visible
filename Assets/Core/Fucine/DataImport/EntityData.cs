using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Noon;

namespace Assets.Core.Fucine.DataImport
{
   public class EntityData
    {
        
        public string Id
        {
            get
            {
                var id= CoreData[NoonConstants.ID];
                return
                    id?.ToString();
            }
        }

        //This should contain Hashtables, ArrayLists and strings all the way down
        public Hashtable CoreData { get; set; }
        public Hashtable LocalisedData { get; set; }

        public EntityData()
        {
            CoreData = new Hashtable();
        }

        public EntityData(Hashtable data)
        {
            CoreData = data;
        }


        public EntityData(Hashtable data, Hashtable localisedData)
        {
            CoreData = data;
            LocalisedData = localisedData;
        }
    }
}
