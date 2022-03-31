using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Constants;
using OrbCreationExtensions;
using static SecretHistories.Fucine.DataImport.PropertyOperationTools;

namespace SecretHistories.Fucine.DataImport
{
    public class EntityData
    {
        public string Id
        {
            get
            {
                var id = ValuesTable[NoonConstants.ID];
                return id?.ToString();
            }
        }

        private readonly string _uniqueId;
        public string UniqueId { get => _uniqueId; }

        public Hashtable ValuesTable { get; set; }

        //grouped all op keys together for clarity
        const string PRIORITY = "$priority";
        const string CONTENTGROUP = "$contentgroup";
        const string MUTE = "$mute";
        const string DERIVES = "$derives";
        const string EXTENDS = "$extends";
        const string EXTENDS_LEGACY = "extends";

        public float GetAndFlushPriority(ContentImportLog log)
        {
            float result = 0;

            if (ValuesTable.ContainsKey(PRIORITY))
            {
                if (float.TryParse(ValuesTable[PRIORITY].ToString(), out result) == false)
                    log.LogProblem($"'{PRIORITY}' of '{this.UniqueId}' is of incorrect type - must be a number");

                ValuesTable.Remove(PRIORITY);
            }

            return result;
        }

        public bool GetAndFlushIgnore(string[] ignoredContentGroups)
        {
            bool result = this.GetArrayListFromEntityData(CONTENTGROUP).ToArray().Intersect(ignoredContentGroups).Any();
            this.ValuesTable.Remove(CONTENTGROUP);
            return result;
        }

        private bool isMuted;
        public void ApplyDataToCollection(Dictionary<string, EntityData> allUniqueEntititesOfType, ContentImportLog log)
        {
            //$ ops are [historically] far from being foolproof, so there are a bunch of safety warnings
            //in some cases, they'll be just clog the log, so this allows to disable some of the [less critical] warnings in a this specific entity definition
            //(from some point of view, this makes them even less foolproof)

            isMuted = ValuesTable.GetBool(MUTE);
            ValuesTable.Remove(MUTE);

            this.InheritMerge(allUniqueEntititesOfType, log);
            this.InheritOverride(allUniqueEntititesOfType, log);

            if (allUniqueEntititesOfType.ContainsKey(this.Id) == false)
            {
                // just adding it as it is - since there's nothing to apply $ops
                allUniqueEntititesOfType.Add(this.Id, this);
                //if the current EntityData's id isn't present in a dictionary, we still want ops to work - for example, if you want to modify derived/extended properties
                ApplyPropertyOperationsOn(this, log);

                //but there's nothing to overwrite or add to, so we skip that part
            }
            else
            {
                //otherwise apply current EntityData's properties and operations to the "core definition" - the one that's kept in the final dictionary
                EntityData coreEntity = allUniqueEntititesOfType[this.Id];

                ApplyPropertyOperationsOn(coreEntity, log);
                OverwriteOrAddPropertiesOfCoreEntity(coreEntity);

                if (!isMuted)
                    log.LogInfo($"Duplicate entity '{this.UniqueId}': merging them (values in second instance will overwrite first, if they overlap)");
            }
        }

        public void InheritMerge(Dictionary<string, EntityData> alreadyLoadedEntities, ContentImportLog log)
        {
            //check this EntityData for entity ids in '$derives'; if there are any, copy any properties from them to this entitymod; if any values overlap, they're are merged, if possible
            //allows for modular definitions like
            //{ "id": "rite_lore_tool_follower", "$derives": [ "rite_slot_lore", "rite_slot_tool", "rite_slot_follower", "rite_slot_desire" ] }
            //wherever the order is important, all parents' values will precede child's; but child can still make use of $prepend
            //(we - obviously - can inherit only from the already loaded entities)

            ArrayList deriveFrom = this.GetArrayListFromEntityData(DERIVES);
            ValuesTable.Remove(DERIVES);

            foreach (string parentId in deriveFrom)
            {
                if (alreadyLoadedEntities.ContainsKey(parentId))
                {
                    //note - all parents' $ ops at this point are resolved and removed from their ValuesTables, so $ ops aren't inheritable (and aren't modifiable) by design
                    EntityData parentEntity = alreadyLoadedEntities[parentId];
                    MergeEntityData(this, parentEntity, log);
                }
                else
                    log.LogWarning($"'{this.UniqueId}' tried to derive from an entity that either doesn't exist or haven't been loaded yet: '{parentId}'");
            }
        }

        public void InheritOverride(Dictionary<string, EntityData> allLoadedEntitiesOfType, ContentImportLog log)
        {
            //check this entitymod for entity ids in 'extends' and '$extends'
            //if there are any, copy properties from identified entity/entities to this entitymod
            //if any values overlap, they're *not* overwritten - either from the initial entity or from successive extensions
            //so child keeps its own defined properties (or overrides parents', however you look at it)
            //we - obviously - can inherit only from already loaded entities
            ArrayList extendFrom = new ArrayList();

            //both "$extends" and "extends" will work; "$extends" is for uniformity [with other $ ops], "extends" - for compatibility

            extendFrom.AddRange(this.GetArrayListFromEntityData(EXTENDS));
            ValuesTable.Remove(EXTENDS);

            extendFrom.AddRange(this.GetArrayListFromEntityData(EXTENDS_LEGACY));
            ValuesTable.Remove(EXTENDS_LEGACY);

            foreach (string parentId in extendFrom)
            {
                if (allLoadedEntitiesOfType.ContainsKey(parentId))
                {
                    EntityData parentEntity = allLoadedEntitiesOfType[parentId];
                    //note - all parents' $ ops at this point are resolved and removed from their ValuesTables, so $ ops aren't inheritable (and aren't modifiable) by design
                    foreach (object key in parentEntity.ValuesTable.Keys)
                        try
                        {
                            //NB parent property should always be deep-copied, see more in CopyValueDeep comments
                            if (this.ValuesTable.ContainsKey(key) == false)
                                ValuesTable.Add(key, CopyValueDeep(parentEntity.ValuesTable[key]));
                        }
                        catch (Exception ex)
                        {
                            log.LogProblem($"Unable to extend property '{key}' of '{this.UniqueId}' from '{parentId}', reason:\n{ex}");
                        }
                }
                else
                    log.LogWarning($"'{this.UniqueId}' tried to extend an entity that either doesn't exist or haven't been loaded yet: '{parentId}'");
            }
        }


        enum ModPropertyOp { Append, Prepend, Plus, Minus, Add, Remove, Prefix, Postfix, Replace, ListEdit, DictEdit, Clear }
        public void ApplyPropertyOperationsOn(EntityData entityToModify, ContentImportLog log)
        {
            //"$prepend"/"$append": list, prepends/appends the list of items to the original list property.
            //"$plus"/"$minus": number, modifies the original number property.
            //"$add": dictionary, overwrites specified fields in the dictionary (or adds them, if they aren't present). Name is a bit misleading, but have to keep it unchanged for legacy reasons.
            //"$remove": list, removes each element in the list from the original property, which can either be a list or a dictionary. Only string values can be removed from a list.
            //"$prefix"/"$postfix": string, prepends/appends the specified string to the original string property.
            //"$replace": dictionary<string, string>, replaces first occurence of the key string with the value string (for each pair).
            //"$clear": removes the property entirely, the value doesn't matter.
            //"$listedit/$dictedit": dictionary, applies any of the $ operations to the nested list or dictionary.
            //                      List entries are targeted as strings with entry number, starting from zero.
            //                      Examples:
            //            "list_of_dictionaries$listedit": { "1$add": { "value": 1 } }
            //            "dictionary$dictedit": { "nested_list$append": [ "value" ] }

            EntityData operatingEntity = this;
            const char OPSEP = '$';

            var allProperties = new ArrayList(operatingEntity.ValuesTable.Keys);
            foreach (string property in allProperties)
            {
                if (property.Contains(OPSEP) == false)
                    continue;

                string[] splitKey = property.Split(OPSEP);
                if (splitKey.Length > 2)
                {
                    log.LogWarning($"Property '{property}' in '{operatingEntity.UniqueId}' contains too many '$', skipping");
                    continue;
                }

                string propertyToModify = splitKey[0];
                string operationName = splitKey[1];
                string operationProperty = property;

                //original property doesn't exist, nothing to modify; actual behaviour will vary from case to case
                if (!operatingEntity.isMuted && entityToModify.ContainsKey(propertyToModify) == false)
                    log.LogWarning($"Trying to perform an operation '{operationName}' on a property '{propertyToModify}' in '{operatingEntity.UniqueId}', but the property doesn't exis - possibly a typo in the property name. Property is initalised, but watch out.");

                if (propertyToModify == NoonConstants.ID) //don't want objects to change their id
                {
                    log.LogWarning($"Trying to apply '{operationName}' operation on ID in '{operatingEntity.UniqueId}', skipping");
                    return;
                }

                if (Enum.TryParse(operationName, true, out ModPropertyOp operation) == false)
                {
                    log.LogWarning($"Unknown property operation '{operationName}' for property '{operationProperty}' in '{operatingEntity.UniqueId}', skipping");
                    return;
                }

                switch (operation)
                {
                    // append: append values to a list.
                    // prepend: prepend values to a list.
                    case ModPropertyOp.Append:
                    case ModPropertyOp.Prepend:
                        {
                            //original property doesn't exist, nothing to modify; just set the value
                            if (entityToModify.ContainsKey(propertyToModify) == false)
                            {
                                entityToModify[propertyToModify] = entityToModify.GetArrayListFromEntityData(operationProperty);
                                break;
                            }

                            ArrayList operationList = operatingEntity.GetArrayListFromEntityData(operationProperty);
                            ArrayList originalList = entityToModify.GetArrayListFromEntityData(propertyToModify);

                            if (operation == ModPropertyOp.Append)
                                originalList.InsertRange(0, operationList);
                            else
                                originalList.AddRange(operationList);

                            break;
                        }

                    // plus: Adds a numerical value to another.
                    // minus: Subtracts a numerical value from another.
                    case ModPropertyOp.Plus:
                    case ModPropertyOp.Minus:
                        {
                            //if modified property doesn't exist, GetFloat returns it as 0
                            float originalValue = entityToModify.ValuesTable.GetFloat(operationProperty, 0f);
                            float change = operatingEntity.ValuesTable.GetFloat(operationProperty);

                            if (operation == ModPropertyOp.Plus)
                                entityToModify[propertyToModify] = originalValue + change;
                            else
                                entityToModify[propertyToModify] = originalValue - change;

                            break;
                        }

                    // add: add or replace keys in a dictionary
                    case ModPropertyOp.Add:
                        {
                            //original property doesn't exist, nothing to modify; just set the value
                            if (entityToModify.ContainsKey(propertyToModify) == false)
                            {
                                entityToModify[propertyToModify] = operatingEntity.GetEntityDataFromEntityData(operationProperty);
                                break;
                            }

                            EntityData originalNestedDictionary = entityToModify.GetEntityDataFromEntityData(propertyToModify);
                            EntityData operationData = operatingEntity.GetEntityDataFromEntityData(operationProperty);

                            if (originalNestedDictionary == null || operationData == null)
                            {
                                log.LogWarning($"Cannot apply '{operation}' to '{propertyToModify}' in '{operatingEntity.UniqueId}': invalid type, must be a dictionary");
                                break;
                            }

                            //actually not just "add" but "OverwriteOrAdd"
                            operationData.OverwriteOrAddPropertiesOfCoreEntity(originalNestedDictionary);

                            break;
                        }

                    // remove: removes items from a dictionary or a list
                    case ModPropertyOp.Remove:
                        {
                            ArrayList valuesToDelete = operatingEntity.GetArrayListFromEntityData(operationProperty);

                            if (entityToModify[propertyToModify] is EntityData)
                            {
                                Hashtable originalNestedDictionary = operatingEntity.GetEntityDataFromEntityData(propertyToModify).ValuesTable;
                                foreach (string toDelete in valuesToDelete)
                                {
                                    if (originalNestedDictionary.ContainsKey(toDelete))
                                        originalNestedDictionary.Remove(toDelete);
                                    else if (!operatingEntity.isMuted)
                                        log.LogWarning($"Failed to delete '{toDelete}' from '{propertyToModify}' in '{operatingEntity.UniqueId}' - entry does not exists");
                                }
                            }
                            else
                            {
                                ArrayList originalNestedList = entityToModify.GetArrayListFromEntityData(propertyToModify);

                                foreach (object toDelete in valuesToDelete)
                                {
                                    if (originalNestedList.Contains(toDelete))
                                        originalNestedList.Remove(toDelete);
                                    else
                                        log.LogWarning($"Failed to delete '{toDelete}' from '{propertyToModify}' in '{operatingEntity.UniqueId}' - entry does not exists");
                                }

                                entityToModify[propertyToModify] = originalNestedList;
                            }

                            break;
                        }

                    // prefix: prepends a string to the original string property
                    // postfix: appends a string to the original string property
                    case ModPropertyOp.Prefix:
                    case ModPropertyOp.Postfix:
                        {
                            //original property doesn't exist, nothing to modify; just set the value
                            if (entityToModify.ContainsKey(propertyToModify) == false)
                            {
                                entityToModify[propertyToModify] = operatingEntity[operationProperty].ToString();
                                break;
                            }

                            if (entityToModify[propertyToModify] is EntityData || entityToModify[propertyToModify] is ArrayList)
                                log.LogWarning($"Failed to apply '{operation}' to '{propertyToModify}' in '{operatingEntity.UniqueId}': target property is of invalid type, must be a string");

                            string originalValue = entityToModify[propertyToModify].ToString();
                            string operationValue = operatingEntity[operationProperty].ToString();

                            if (operation == ModPropertyOp.Prefix)
                                entityToModify[propertyToModify] = operationValue + originalValue;
                            else
                                entityToModify[propertyToModify] = originalValue + operationValue;

                            break;
                        }
                    case ModPropertyOp.Replace:
                        {
                            //original property doesn't exist, nothing to replace
                            if (entityToModify.ContainsKey(propertyToModify) == false)
                                break;

                            EntityData operationValue = operatingEntity.GetEntityDataFromEntityData(operationProperty);
                            if (operationValue == null)
                                log.LogWarning($"Failed to apply '{operation}' to '{propertyToModify}' in '{operatingEntity.UniqueId}': operation property is of invalid type, must be a dictionary");

                            if (entityToModify[propertyToModify] is EntityData || entityToModify[propertyToModify] is ArrayList)
                                log.LogWarning($"Failed to apply '{operation}' to '{propertyToModify}' in '{operatingEntity.UniqueId}': target property is of invalid type, must be a string");

                            string originalValue = entityToModify[propertyToModify].ToString();
                            
                            foreach(string toReplace in operationValue.ValuesTable.Keys)
                            {
                                string replaceWith = operationValue.ValuesTable[toReplace].ToString();

                                //we want to replace only the first occurence, that's why not just .Replace(), but all these
                                int position = originalValue.IndexOf(toReplace);
                                string result = originalValue.Remove(position, toReplace.Length).Insert(position, replaceWith);

                                entityToModify[propertyToModify] = result;
                            }

                            break;
                        }

                    // clear: removes property entirely; op content doesn't matter
                    case ModPropertyOp.Clear:
                        {
                            if (entityToModify.ContainsKey(propertyToModify))
                                entityToModify.ValuesTable.Remove(propertyToModify);
                            break;
                        }
                    case ModPropertyOp.ListEdit:
                    case ModPropertyOp.DictEdit:
                        {
                            EntityData processData = operatingEntity.GetEntityDataFromEntityData(operationProperty);
                            if (processData == null)
                            {
                                log.LogWarning($"Failed to apply '{operation}' to '{propertyToModify}' in '{operatingEntity.UniqueId}': '{operationProperty}' is of invalid type, must be a dictionary");
                                break;
                            }

                            EntityData nestedEntity;
                            if (entityToModify.ContainsKey(propertyToModify))
                            {
                                nestedEntity = new EntityData($"{entityToModify.UniqueId}>{property}", new Hashtable());
                            }
                            if (operation == ModPropertyOp.ListEdit)
                            {
                                nestedEntity = entityToModify.GetArrayListFromEntityData(propertyToModify).ToEntityData(entityToModify.UniqueId, propertyToModify);
                            }
                            else
                            {
                                EntityData originalNestedDictionary = entityToModify.GetEntityDataFromEntityData(propertyToModify);
                                if (originalNestedDictionary == null)
                                {
                                    log.LogWarning($"Failed to apply '{operation}' to '{propertyToModify}' in '{operatingEntity.UniqueId}': target property is of invalid type, must be a dictionary");
                                    break;
                                }

                                nestedEntity = originalNestedDictionary;
                            }

                            processData.ApplyPropertyOperationsOn(nestedEntity, log);
                            processData.OverwriteOrAddPropertiesOfCoreEntity(nestedEntity);

                            if (operation == ModPropertyOp.ListEdit)
                                entityToModify[propertyToModify] = nestedEntity.ToArrayList(log);
                            else
                                entityToModify[propertyToModify] = nestedEntity;

                            break;
                        }
                    default:
                        {
                            log.LogWarning($"Unknown property operation {operation} for property '{operationProperty}' in '{operatingEntity.UniqueId}', skipping");
                            break;
                        }
                }

                operatingEntity.ValuesTable.Remove(operationProperty);
            }
        }



        private void OverwriteOrAddPropertiesOfCoreEntity(EntityData coreEntity)
        {
            foreach (DictionaryEntry entry in this.ValuesTable)
                if (coreEntity.ValuesTable.ContainsKey(entry.Key))
                    coreEntity.ValuesTable[entry.Key] = entry.Value;
                else
                    coreEntity.ValuesTable.Add(entry.Key, entry.Value);
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

        public EntityData(string uniqueId, Hashtable data)
        {
            _uniqueId = uniqueId;
            ValuesTable = data;
        }

    }

}
