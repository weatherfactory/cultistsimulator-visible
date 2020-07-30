using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Noon;
using Hashtable = System.Collections.Hashtable;

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

        /// <summary>
        /// gets entity ids to extend *and then removes it* so it doesn't show up as an invalid property later
        /// </summary>
        /// <returns></returns>
        public List<string> FlushEntityIdsToExtend()
        {
            if (ValuesTable.ContainsKey(NoonConstants.EXTENDS))
            {
                var ids = ValuesTable[NoonConstants.EXTENDS] as ArrayList;
                ValuesTable.Remove(NoonConstants.EXTENDS);

                if(ids==null)
                    return new List<string>();

                return ids.Cast<string>().ToList();
            }
            else return new List<string>();

        }

        /// <summary>
        /// all the values that we should copy across with extends or similar, excluding meta and mod values
        /// </summary>
        /// <returns></returns>
        public Hashtable GetCopiableValues()
        {
            Hashtable copiableValues=new Hashtable();
            foreach(string key in ValuesTable.Keys)
                if(key!=NoonConstants.ID && key!=NoonConstants.EXTENDS)
                    copiableValues.Add(key,ValuesTable[key]);

            return copiableValues;
        }

        /// <summary>
        /// return true if adding successfully, false if the key's already there
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryAdd(object key, object value)
        {
            if (ValuesTable.ContainsKey(key))
                return false;

            ValuesTable.Add(key,value);
            return true;

        }



        /// <summary>
        /// True if we overwrite, false if we add
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if we overwrite, false if we add</returns>
        public bool OverwriteOrAdd(object key, object value)
        {
            if (ValuesTable.ContainsKey(key))
            {
                ValuesTable[key] = value;
                return true;
            }
            else
            {
                ValuesTable.Add(key,value);
                return false;
            }

        }

        public Hashtable ValuesTable { get; set; }

        public EntityData GetEntityDataFromValueTable(string key)
        {
            if (!ValuesTable.ContainsKey(key))
                return null;
            else
                //this will be null if it's not actually EntityData in there
                return ValuesTable[key] as EntityData;
        }


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
