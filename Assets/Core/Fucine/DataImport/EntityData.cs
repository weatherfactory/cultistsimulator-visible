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
                var id= ValuesTable[NoonConstants.ID];
                return
                    id?.ToString();
            }
        }

        public string UniqueId
        {
            get
            {
                var uid = ValuesTable["uid"];
                return
                    uid?.ToString();
            }
        }


        public Hashtable ValuesTable { get; set; }

        public EntityData()
        {
            ValuesTable = new Hashtable();
        }

        public EntityData(Hashtable data)
        {
            ValuesTable = data;
        }


    }
}
