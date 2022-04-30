using System;
using System.Collections;
using System.Collections.Generic;
using OrbCreationExtensions;
using SecretHistories.Constants;
using System.Linq;

namespace SecretHistories.Fucine.DataImport
{
    public static class EntityDataImportExtensions
    {
        //grouped all entity-level ops together for clarity
        const string PRIORITY = "$priority";
        const string CONTENTGROUP = "$contentgroup";
        const string MUTE = "$mute";
        const string DERIVES = "$derives";
        const string EXTENDS = "$extends";
        const string EXTENDS_LEGACY = "extends";

        public static float FlushPriorityFromEntityData(this EntityData entityData, ContentImportLog log)
        {
            float result = 0;

            if (entityData.ValuesTable.ContainsKey(PRIORITY))
            {
                if (float.TryParse(entityData.ValuesTable[PRIORITY].ToString(), out result) == false)
                    log.LogProblem($"'{PRIORITY}' of '{entityData.UniqueId}' is of incorrect type - must be a number");

                entityData.ValuesTable.Remove(PRIORITY);
            }

            return result;
        }

        public static bool FlushIgnoreFromEntityData(this EntityData entityData, string[] ignoredContentGroups)
        {
            entityData.contentGroups = entityData.GetArrayListFromEntityData(CONTENTGROUP).Cast<string>().ToArray();
            bool result = entityData.contentGroups.Intersect(ignoredContentGroups).Any();
            entityData.ValuesTable.Remove(CONTENTGROUP);
            
            return result;
        }

        public static void FlushMuteFromEntityData(this EntityData entityData)
        {
            entityData.isMuted = entityData.ValuesTable.GetBool(MUTE);
            entityData.ValuesTable.Remove(MUTE);
        }

        public static void InheritMerge(this EntityData childEntity, Dictionary<string, EntityData> alreadyLoadedEntities, ContentImportLog log)
        {
            //check this EntityData for entity ids in '$derives'; if there are any, copy any properties from them to this entitymod; if any values overlap, they're are merged, if possible
            //allows for modular definitions like
            //{ "id": "rite_lore_tool_follower", "$derives": [ "rite_slot_lore", "rite_slot_tool", "rite_slot_follower", "rite_slot_desire" ] }
            //wherever the order is important, all parents' values will precede child's; but child can still make use of $prepend
            //(we - obviously - can inherit only from the already loaded entities)
            ArrayList deriveFrom = childEntity.GetArrayListFromEntityData(DERIVES);
            childEntity.ValuesTable.Remove(DERIVES);

            foreach (string parentId in deriveFrom)
            {
                if (alreadyLoadedEntities.ContainsKey(parentId))
                {
                    //note - all parents' $ ops at this point are resolved and removed from their ValuesTables, so $ ops aren't inheritable (and aren't modifiable) by design
                    EntityData parentEntity = alreadyLoadedEntities[parentId];
                    MergeEntityData(childEntity, parentEntity, log);
                }
                else
                    log.LogWarning($"'{childEntity.UniqueId}' tried to derive from an entity that either doesn't exist or haven't been loaded yet: '{parentId}'");
            }
        }

        public static void InheritOverride(this EntityData childEntity, Dictionary<string, EntityData> allLoadedEntitiesOfType, ContentImportLog log)
        {
            //check this entitymod for entity ids in 'extends' and '$extends'
            //if there are any, copy properties from identified entity/entities to this entitymod
            //if any values overlap, they're *not* overwritten - either from the initial entity or from successive extensions
            //so child keeps its own defined properties (or overrides parents', however you look at it)
            //we - obviously - can inherit only from already loaded entities
            ArrayList extendFrom = new ArrayList();

            //both "$extends" and "extends" will work; "$extends" is for uniformity [with other $ ops], "extends" - for compatibility
            extendFrom.AddRange(childEntity.GetArrayListFromEntityData(EXTENDS));
            childEntity.ValuesTable.Remove(EXTENDS);

            extendFrom.AddRange(childEntity.GetArrayListFromEntityData(EXTENDS_LEGACY));
            childEntity.ValuesTable.Remove(EXTENDS_LEGACY);

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
                            if (childEntity.ValuesTable.ContainsKey(key) == false)
                                childEntity.ValuesTable.Add(key, CopyValueDeep(parentEntity.ValuesTable[key]));
                        }
                        catch (Exception ex)
                        {
                            log.LogProblem($"Unable to extend property '{key}' of '{childEntity.UniqueId}' from '{parentId}', reason:\n{ex}");
                        }
                }
                else
                    log.LogWarning($"'{childEntity.UniqueId}' tried to extend an entity that either doesn't exist or haven't been loaded yet: '{parentId}'");
            }
        }

        public static void OverwriteOrAddPropertiesOfCoreEntity(this EntityData fromEntityData, EntityData coreEntity)
        {
            foreach (DictionaryEntry entry in fromEntityData.ValuesTable)
                coreEntity.ValuesTable[entry.Key] = entry.Value;
        }

        enum ModPropertyOp { Append, Prepend, Plus, Minus, Add, Remove, Prefix, Postfix, Replace, ReplaceLast, ListEdit, DictEdit, Clear }
        public static void ApplyPropertyOperationsOn(this EntityData operatingEntity, EntityData entityToModify, ContentImportLog log)
        {
            //"$prepend"/"$append": list, prepends/appends the list of items to the original list property.
            //"$plus"/"$minus": number, modifies the original number property.
            //"$add": dictionary, overwrites specified fields in the dictionary (or adds them, if they aren't present). Name is a bit misleading, but have to keep it unchanged for legacy reasons.
            //"$remove": list, removes each element in the list from the original property, which can either be a list or a dictionary. Only string values can be removed from a list.
            //"$prefix"/"$postfix": string, prepends/appends the specified string to the original string property.
            //"$replace"/"$replacelast": list<dictionary<string, string>>, replaces first/last occurence of the key string with the value string (for each pair).
            //                           Warning: Keys (values that'll be changed) are case-insensitive.
            //"$clear": removes the property entirely, the value doesn't matter.
            //"$listedit/$dictedit": dictionary, applies any of the $ operations to the nested list or dictionary.
            //                      List entries are targeted as strings with entry number, starting from zero.
            //                      Examples:
            //            "list_of_dictionaries$listedit": { "0$add": { "value": 1 } }
            //            "dictionary$dictedit": { "nested_list$append": [ "value" ] }
            //$listedit also allows to insert new entries to the list (even in-between the existing ones - by using decimal values)

            //  EntityData operatingEntity = this;
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

                if (propertyToModify == NoonConstants.ID) //this shouldn't do any harm
                    log.LogWarning($"Trying to apply '{operationName}' operation on ID in '{operatingEntity.UniqueId}'. Why?");

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
                            //original property doesn't exist, nothing to modify; skip
                            if (entityToModify.ContainsKey(propertyToModify) == false)
                            {
                                break;
                            }

                            ArrayList valuesToDelete = operatingEntity.GetArrayListFromEntityData(operationProperty);

                            if (entityToModify[propertyToModify] is EntityData)
                            {
                                Hashtable originalNestedDictionary = entityToModify.GetEntityDataFromEntityData(propertyToModify).ValuesTable;

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
                    case ModPropertyOp.ReplaceLast:
                        {
                            //original property doesn't exist, nothing to replace
                            if (entityToModify.ContainsKey(propertyToModify) == false)
                                break;

                            if (entityToModify[propertyToModify] is EntityData || entityToModify[propertyToModify] is ArrayList)
                                log.LogWarning($"Failed to apply '{operation}' to '{propertyToModify}' in '{operatingEntity.UniqueId}': target property is of invalid type, must be a string.");

                            ArrayList replacementsList = operatingEntity.GetArrayListFromEntityData(operationProperty);

                            //before appliyng have to check whether all replacements are defined as dicts
                            if (!allValuesAreDicts(replacementsList))
                            {
                                log.LogWarning($"Failed to apply '{operation}' to '{propertyToModify}' in '{operatingEntity.UniqueId}': operation entry is of invalid type, must be a dictionary.");
                                break;
                            }
                            bool allValuesAreDicts(ArrayList values)
                            {
                                foreach (object value in values)
                                    if (!(value is EntityData))
                                        return false;

                                return true;
                            }

                            Func<string, string, int> indexOf;
                            if (operation == ModPropertyOp.Replace)
                                indexOf = (str, substring) => str.IndexOf(substring, StringComparison.OrdinalIgnoreCase);
                            else
                                indexOf = (str, substring) => str.LastIndexOf(substring, StringComparison.OrdinalIgnoreCase);
                            //using ordinal comparison for max fidelity; have to use IgnoreCase since keys automatically become lowercase when parsing JSON
                            //this shouldn't be a tragedy since modder should have a good idea what they're changing anyway

                            string originalString = entityToModify[propertyToModify].ToString();
                            foreach (EntityData replacements in replacementsList)
                                foreach (string replaceSubstring in replacements.ValuesTable.Keys)
                                {
                                    int position = indexOf(originalString, replaceSubstring);
                                    if (position == -1)
                                        continue;

                                    string replaceWith = replacements.ValuesTable[replaceSubstring].ToString();
                                    originalString = originalString.Remove(position, replaceSubstring.Length).Insert(position, replaceWith);
                                }

                            entityToModify[propertyToModify] = originalString;
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


        /// <summary>
        /// Merges collections; leaves values and strings untouched.
        /// </summary>
        public static object MergeValues(object destination, object source, ContentImportLog log)
        {
            //all value/string types are untouched (explanation below)
            //all lists are joined - including ones that are nested inside dictionaries
            //in dictionaries - overlapping properties are joined (by the same rules), missing properties are added
            //NB 'source' should always be deep-copied, see more in CopyValueDeep comments

            if (destination.GetType() == source.GetType())
            {
                if (destination is ArrayList)
                {
                    //both values are lists; just insert a copy of source at the start of destination
                    (destination as ArrayList).InsertRange(0, CopyValueDeep(source) as ArrayList);
                    return destination;
                }
                else if (destination is EntityData)
                {
                    //both values are EntityData; merge them
                    MergeEntityData(destination as EntityData, source as EntityData, log);
                    return destination;
                }
                else
                {
                    //a rare occasion of goto in the wild; value types merging is a tickly subject, so I want to keep it in one place
                    goto PropertyIsNumberOrString;
                }
            }
            else if (destination is ArrayList || source is ArrayList)
            {
                //if only one of the properties is a list, another is interpreted as "non-wrapped single entry list"

                if (destination is ArrayList)
                {
                    //destination is a list; insert a copy of source's value at its start
                    (destination as ArrayList).Insert(0, CopyValueDeep(source));
                    return destination;
                }
                else
                {
                    //source is a list; copy it and add destination's value to its end; return the resulting list
                    ArrayList result = CopyValueDeep(source) as ArrayList;
                    result.Add(destination);
                    return result;
                }
            }
            else if (destination is EntityData || source is EntityData)
            {
                //EntityData can't be merged with a value type
                throw new Exception("Can't merge a dictionary with a value type");
            }

        PropertyIsNumberOrString:
            //value/string types shouldn't be merged - while it can make sense for some properties, stuff like "icon" aren't mergable in any sane way
            //thus, the value just remains the same
            return destination;
        }

        public static void MergeEntityData(EntityData destination, EntityData source, ContentImportLog log)
        {
            foreach (string key in source.ValuesTable.Keys)
            {
                if (destination.ValuesTable.ContainsKey(key))
                {
                    try
                    {
                        destination.ValuesTable[key] = MergeValues(destination.ValuesTable[key], source.ValuesTable[key], log);
                    }
                    catch (Exception ex)
                    {
                        log.LogProblem($"Unable to merge property '{key}' of entities '{destination.Id}' and '{source.Id}, error:\n {ex}'");
                    }
                }
                else
                    destination.ValuesTable.Add(key, CopyValueDeep(source.ValuesTable[key]));
            }

        }

        /// <summary>
        /// Creates a deep copy of an object.
        /// </summary>
        public static object CopyValueDeep(object source)
        {
            //wherever we use inheritance, we have to make a deep copy of inherited value to prevent the same sub-entity from appearing in several different root entities
            //(to, in turn, prevent any modifications/deletions of said sub-entity from spreading to its siblings/copies)
            //everything is defined as either a list, dictionary, or non-reference value

            if (source is ArrayList)
            {
                ArrayList result = new ArrayList();
                foreach (object entry in source as ArrayList)
                    result.Add(CopyValueDeep(entry));

                return result;
            }
            else if (source is EntityData)
            {
                EntityData sourceData = source as EntityData;
                EntityData result = new EntityData(); //not just 'new EntityData(sourceData.ValuesTable)' since we need to make a deep copy of everything

                foreach (DictionaryEntry entry in sourceData.ValuesTable)
                    result.ValuesTable.Add(CopyValueDeep(entry.Key), CopyValueDeep(entry.Value));

                return result;
            }
            else
                return source;
        }

        public static EntityData GetEntityDataFromEntityData(this EntityData table, string key)
        {
            if (!table.ContainsKey(key))
                return null;
            else
                //this will be null if it's not actually EntityData in there
                return table[key] as EntityData;
        }

        /// <summary>
        /// Returns an ArrayList from EntityData even if the property is not a list (but supposed to be). Returns empty ArrayList if EntityData doesn't contain the desired property.
        /// </summary>
        public static ArrayList GetArrayListFromEntityData(this EntityData table, string key)
        {
            //wrapping this behaviour in this method in case it turns out to be unwanted
            //to disable it, just replace GetArrayList(key, true) here to just GetArrayList(key)
            return table.ValuesTable.GetArrayList(key, true) ?? new ArrayList();
        }

        /// <summary>
        /// Converts ArrayList to EntityData with ValuesTable like { "0": value, "1": value, etc }
        /// </summary>
        public static EntityData ToEntityData(this ArrayList list, string parentUniqueId, string property)
        {
            Hashtable table = new Hashtable();
            int i = 0;
            while (i < list.Count)
            {
                table[i.ToString()] = list[i];
                i++;
            }

            EntityData result = new EntityData($"{parentUniqueId}>{property}", table);
            return new EntityData(table);
        }

        public static ArrayList ToArrayList(this EntityData entityData, ContentImportLog log)
        {
            ArrayList keysOrdered = new ArrayList(entityData.ValuesTable.Keys);

            //converting the keys back to their numeric state 
            for (int n = 0; n < keysOrdered.Count; n++)
                if (float.TryParse(keysOrdered[n].ToString(), out float keyAsFloat))
                    keysOrdered[n] = keyAsFloat;
                else
                    log.LogProblem("Non-numerical value in a $listedit");

            keysOrdered.Sort();

            ArrayList list = new ArrayList();
            foreach (object key in keysOrdered)
                list.Add(entityData[key.ToString()]);

            return list;
        }


        public static bool propertyIsStringOperation(string propertyKey)
        {
            return propertyKey.Contains("$prefix")
                || propertyKey.Contains("$postfix")
                || propertyKey.Contains("$replace")
                || propertyKey.Contains("$listedit")
                || propertyKey.Contains("$dictedit");
        }
    }
}
