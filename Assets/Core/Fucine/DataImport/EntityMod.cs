using System.Collections;
using System.Collections.Generic;
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

        public void ApplyModTo(Dictionary<string, EntityData> coreEntitiesDataDictionary,ContentImportLog log)
        {
            //check this entitymod for entity ids in 'extends'
            //if there are any, copy values from identified entity/entities entities to this entitymod
            //if any values overlap, they'll overwrite it
            foreach(var e in _modData.GetEntityIdsToExtend())
            {
                if(coreEntitiesDataDictionary.TryGetValue(e,out var dataToExtendWith))
                {
                    var copiableValues = dataToExtendWith.GetCopiableValues();
                    foreach (var k in copiableValues.Keys)
                        _modData.OverwriteOrAdd(k, copiableValues[k]);
                }
                else
                {
                    log.LogWarning($"{_modData.Id} tried to extend from an entity that doesn't exist: {e}" );
                }

            }

            ProcessPropertyOperations(log);




            //all mod operations complete. Now either overwrite an existing item with that id, or add a new one 
            if (coreEntitiesDataDictionary.ContainsKey(_modData.Id))
            {
                _modData.ValuesTable = _modData.GetNonModlyValues();
                //    If a mod object id does exist in original data - completely overwrite that entity in data
                coreEntitiesDataDictionary[_modData.Id]=_modData;

            }
            else
            {
                _modData.ValuesTable = _modData.GetNonModlyValues();
                //If a mod object id doesn't exist in original data - create a new entity and add it to data			
                coreEntitiesDataDictionary.Add(_modData.Id,_modData);
            }
       
        }

        private void ProcessPropertyOperations(ContentImportLog log)
        {
            //"$append": appends a list of items to the original list property.
            //"$prepend": prepends a list of items to the original list property.
            //"$plus": adds the specified number to the original number property.
            //"$minus": subtracts the specified number from the original number property.
            //"$extend": extends a dictionary with the specified properties.
            //"$remove": removes each element in the list from the original property, which can either be a list or a dictionary.


            var itemId = _modData.ValuesTable.GetString("id");
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
                if (!_modData.ValuesTable.ContainsKey(propertyToAlterKey))
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
                            var originalValue = _modData.ValuesTable.GetArrayList(propertyToAlterKey);
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
                            _modData.ValuesTable[propertyToAlterKey] = value + newValue * modifier;
                            break;
                        }

                    // extend: add or replace keys in a dictionary
                    case "extend":
                        {
                            var value = _modData.ValuesTable.GetHashtable(propertyToAlterKey);
                            var newValue = _modData.ValuesTable.GetHashtable(key);
                            if (value == null || newValue == null)
                            {
                                log.LogWarning(
                                    "Cannot apply '{operation}' to '" + propertyToAlterKey + "' in '" + itemId + "': invalid type, must be a dictionary");
                                continue;
                            }

                            value.AddHashtable(newValue, true);

                            break;
                        }

                    // remove: removes items from a dictionary or a list
                    case "remove":
                        {
                            var newValue = _modData.ValuesTable.GetArrayList(key);
                            if (newValue == null)
                            {
                                log.LogWarning(
                                    "Invalid value for '" + key + "' in '" + itemId + "': invalid type, must be a list");
                                continue;
                            }

                            if (!_modData.ValuesTable.ContainsKey(propertyToAlterKey))
                            {
                                log.LogWarning(
                                    "Cannot apply '{operation}' to '" + propertyToAlterKey + "' in '" + itemId + "': failed to find '" + propertyToAlterKey + "'");
                                continue;
                            }

                            object originalPropertyValue = _modData.ValuesTable[propertyToAlterKey];
                            if (originalPropertyValue.GetType() == typeof(Hashtable))
                            {
                                var value = _modData.ValuesTable.GetHashtable(propertyToAlterKey);
                                foreach (string toDelete in newValue)
                                {
                                    if (value.ContainsKey(toDelete))
                                        value.Remove(toDelete);
                                    else
                                        log.LogWarning(
                                            "Failed to delete '" + toDelete + "' from '" + propertyToAlterKey +
                                            "' in '" + itemId + "'");
                                }
                            }
                            else if (originalPropertyValue.GetType() == typeof(ArrayList))
                            {
                                var value = _modData.ValuesTable.GetArrayList(propertyToAlterKey);
                                foreach (string toDelete in newValue)
                                {
                                    if (value.Contains(toDelete))
                                        value.Remove(toDelete);
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