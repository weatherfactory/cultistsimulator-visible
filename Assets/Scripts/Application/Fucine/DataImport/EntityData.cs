using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Constants;
using Hashtable = System.Collections.Hashtable;

namespace SecretHistories.Fucine.DataImport
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
        public bool isMuted { get; set; }
        //contentGroups are used only (to determine ignore, but I'm storing them in case they'll be required for something else)
        public string[] contentGroups { get; set; }

        public void ApplyDataToCollection(Dictionary<string, EntityData> allUniqueEntititesOfType, ContentImportLog log)
        {
            //$ ops are [historically] far from being foolproof, so there are a bunch of safety warnings
            //in some cases, they'll just clog the log, so this allows to disable some of the [less critical] warnings in a this specific entity definition
            //(from some point of view, this makes them even less foolproof)
            this.FlushMuteFromEntityData();

            this.InheritMerge(allUniqueEntititesOfType, log);
            this.InheritOverride(allUniqueEntititesOfType, log);

            if (allUniqueEntititesOfType.ContainsKey(this.Id) == false)
            {
                // just adding it as it is - since there's nothing to apply $ops
                allUniqueEntititesOfType.Add(this.Id, this);
                //if the current EntityData's id isn't present in a dictionary, we still want ops to work - for example, if you want to modify derived/extended properties
                this.ApplyPropertyOperationsOn(this, log);

                //but there's nothing to overwrite or add to, so we skip that part
            }
            else
            {
                //otherwise apply current EntityData's properties and operations to the "core definition" - the one that's kept in the final dictionary
                EntityData coreEntity = allUniqueEntititesOfType[this.Id];

                this.ApplyPropertyOperationsOn(coreEntity, log);
                this.OverwriteOrAddPropertiesOfCoreEntity(coreEntity);

                if (!isMuted)
                    log.LogInfo($"Duplicate entity '{this.UniqueId}': merging them (values in second instance will overwrite first, if they overlap)");
            }
        }

        public bool ContainsKey(object key)
        {
            return this.ValuesTable.ContainsKey(key);
        }

        public object this[object key]
        {
            get { return this.ValuesTable[key]; }
            set { this.ValuesTable[key] = value; }
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
