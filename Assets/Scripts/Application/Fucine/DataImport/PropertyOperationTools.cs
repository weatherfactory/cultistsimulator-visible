using System;
using System.Collections;
using OrbCreationExtensions;
using SecretHistories.Constants;

namespace SecretHistories.Fucine.DataImport
{
    public static class PropertyOperationTools
    {
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
    }
}
