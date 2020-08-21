using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Fucine.DataImport;
using Noon;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class EntityMod
    {
        private readonly EntityData _modData;

        public EntityMod(EntityData modData)
        {
            _modData = modData;
        }

        public void ApplyModTo(Dictionary<string, EntityData> allCoreEntitiesOfType,ContentImportLog log)
        {
            //check this entitymod for entity ids in 'extends'
            //if there are any, copy values from identified entity/entities entities to this entitymod
            //if any values overlap, they're *not* overwritten - either from the initial entity or from successive extensions
            //this gives the mod entity priority on specifying values

            var entityIdsToExtend = _modData.FlushEntityIdsToExtend();

            if(entityIdsToExtend.Any())
            {
            
                foreach(var e in entityIdsToExtend)
                {
                    if(allCoreEntitiesOfType.TryGetValue(e,out var dataToExtendWith))
                    {
                        foreach (var k in dataToExtendWith.ValuesTable.Keys)
                            _modData.TryAdd(k, dataToExtendWith.ValuesTable[k]);
                    }
                    else
                    {
                        log.LogWarning($"{_modData.Id} tried to extend from an entity that doesn't exist: {e}" );
                    }
                }


            }
            //mod data has now been populated with any values from the original entity.

            

            //1. Merge or overwrite any valid properties. Ignore property-operation properties for now.
            
            //all mod operations complete. Now either merge-overwrite an existing item with that id, or add a new one 
            if (allCoreEntitiesOfType.ContainsKey(_modData.Id))
            {
                var mergeDataItem = new EntityData(_modData.UniqueId,allCoreEntitiesOfType[_modData.Id].ValuesTable);

                //1. Apply property operations to the core data.
                ProcessPropertyOperationsFromEntityMod(log, mergeDataItem);

                OverwriteOrAddPropertiesFromEntityMod(mergeDataItem);
                

                allCoreEntitiesOfType[_modData.Id] = mergeDataItem;
            }
            else
            {
                //If a mod object id doesn't exist in original data - create a new entity and add it to data			
                allCoreEntitiesOfType.Add(_modData.Id, _modData);
            }






        }

        private void OverwriteOrAddPropertiesFromEntityMod(EntityData mergeDataItem)
        {
            foreach (string key in _modData.ValuesTable.Keys)
                mergeDataItem.OverwriteOrAdd(key, _modData.ValuesTable[key]);
        }

        private void ProcessPropertyOperationsFromEntityMod(ContentImportLog log,EntityData entityDataToModify)
        {
            //"$append": appends a list of items to the original list property.
            //"$prepend": prepends a list of items to the original list property.
            //"$plus": adds the specified number to the original number property.
            //"$minus": subtracts the specified number from the original number property.
            //"$add": extends a dictionary with the specified properties.
            //"$remove": removes each element in the list from the original property, which can either be a list or a dictionary.


            var itemId = _modData.Id;
            var keys = new ArrayList(_modData.ValuesTable.Keys);
            const char MODSEP = '$';
            foreach (string key in keys)
            {
                if(key.IndexOf(MODSEP)<0)
                    continue;

                var splitKey = key.Split(MODSEP);
                

                if (splitKey.Length > 2)
                {
                    log.LogWarning(
                         "Property '" + key + "' in '" + itemId + "' contains too many '$', skipping");
                    continue;
                }

                var propertyToAlterKey = splitKey[0];
                if (!entityDataToModify.ValuesTable.ContainsKey(propertyToAlterKey))
                {
                    log.LogWarning(
                        "Unknown property '" + propertyToAlterKey + "' for property '" + key + "' in '" + itemId + "', skipping");
                    continue;
                }
                var operation = splitKey[1];
                switch (operation)
                {
                    // append: append values to a list
                    // prepend: prepend values to a list
                    case "append":
                    case "prepend":
                        {
                            var originalValue = entityDataToModify.ValuesTable.GetArrayList(propertyToAlterKey);
                            var newValue = _modData.ValuesTable.GetArrayList(key);
                            if (originalValue == null || newValue == null)
                            {
                                log.LogWarning(
                                    "Cannot apply '{operation}' to '" + propertyToAlterKey + "' in '" + itemId + "': invalid type, must be a list");
                                continue;
                            }

                            if (operation == "append")
                            {
                                originalValue.AddRange(newValue);
                            }
                            else
                            {
                                originalValue.InsertRange(0, newValue);
                            }

                            break;
                        }

                    // plus: Adds a numerical value to another.
                    // minus: Subtracts a numerical value from another.
                    case "plus":
                    case "minus":
                        {
                            var value = _modData.ValuesTable.GetFloat(propertyToAlterKey);
                            var newValue = _modData.ValuesTable.GetFloat(key);

                            var modifier = operation == "plus" ? 1 : -1;
                            entityDataToModify.ValuesTable[propertyToAlterKey] = value + newValue * modifier;
                            break;
                        }

                    // add: add or replace keys in a dictionary
                    case "add":
                    {
                        var existingValuesData = entityDataToModify.GetEntityDataFromValueTable(propertyToAlterKey);
                            var newValuesData = _modData.GetEntityDataFromValueTable(key);
                            if (existingValuesData == null || newValuesData == null)
                            {
                                log.LogWarning(
                                    "Cannot apply '{operation}' to '" + propertyToAlterKey + "' in '" + itemId + "': invalid type, must be a dictionary");
                                continue;
                            }

                            foreach (var newValueKey in newValuesData.ValuesTable.Keys)
                                existingValuesData.OverwriteOrAdd(newValueKey, newValuesData.ValuesTable[newValueKey]);

                            break;
                        }

                    // remove: removes items from a dictionary or a list
                    case "remove":
                        {
                            var valuesToDelete = entityDataToModify.ValuesTable.GetArrayList(key);
                            if (valuesToDelete == null)
                            {
                                log.LogWarning(
                                    "Invalid value for '" + key + "' in '" + itemId + "': invalid type, must be a list");
                                continue;
                            }

                            if (!entityDataToModify.ValuesTable.ContainsKey(propertyToAlterKey))
                            {
                                log.LogWarning(
                                    "Cannot apply '{operation}' to '" + propertyToAlterKey + "' in '" + itemId + "': failed to find '" + propertyToAlterKey + "'");
                                continue;
                            }

                            object originalPropertyValues = entityDataToModify.ValuesTable[propertyToAlterKey];
                            if (originalPropertyValues.GetType() == typeof(EntityData))
                            {
                                var originalValuesHashtable = entityDataToModify.GetEntityDataFromValueTable(propertyToAlterKey).ValuesTable;
                                foreach (string valueToDelete in valuesToDelete)
                                {
                                    if (originalValuesHashtable.ContainsKey(valueToDelete))
                                        originalValuesHashtable.Remove(valueToDelete);
                                    else
                                        log.LogWarning(
                                            "Failed to delete '" + valueToDelete + "' from '" + propertyToAlterKey +
                                            "' in '" + itemId + "'");
                                }
                            }
                            else if (originalPropertyValues.GetType() == typeof(ArrayList))
                            {
                                var originalValuesArrayList = entityDataToModify.ValuesTable.GetArrayList(propertyToAlterKey);
                                foreach (string toDelete in valuesToDelete)
                                {
                                    if (originalValuesArrayList.Contains(toDelete))
                                        originalValuesArrayList.Remove(toDelete);
                                    else
                                        log.LogWarning(
                                            "Failed to delete '" + toDelete + "' from '" + propertyToAlterKey + "' in '" + itemId + "'");
                                }
                            }
                            else
                            {
                                log.LogWarning(
                                    "Cannot apply '{operation}' to '" + propertyToAlterKey + "' in '" + itemId + "': invalid type, must be a dictionary or a list");
                            }

                            break;
                        }
                    default:
                        log.LogWarning(
                            "Unknown operation '{operation}' for property '" + key + "' in '" + itemId + "', skipping");
                        continue;
                }

                // Remove the property once it has been processed, to avoid warnings from the content importer
                _modData.ValuesTable.Remove(key);
            }
        }

    }
}