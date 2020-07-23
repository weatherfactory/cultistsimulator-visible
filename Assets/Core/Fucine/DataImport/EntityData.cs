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
        private readonly string _uniqueId;

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
            get => _uniqueId;
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

        public EntityData(string uniqueId,Hashtable data)
        {
            _uniqueId = uniqueId;
            ValuesTable = data;
        }


    }
}
