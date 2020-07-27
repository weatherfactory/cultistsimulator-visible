using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Noon;
using OrbCreationExtensions;
using UnityEngine;

namespace Assets.Core
{
    class ContentImportForMods
    {
        public ArrayList UpdateEntityDataFromMods(ArrayList items, Type forEntityType)
        {

            FucineImportable importableAttributeForEntityType =
                (FucineImportable)forEntityType.GetCustomAttribute(typeof(FucineImportable), false);

            var modManager = Registry.Retrieve<ModManager>();
            var moddedItems = modManager.LoadContentForEntityType(forEntityType);
            foreach (var moddedItem in moddedItems)
            {
                var moddedItemId = moddedItem.GetString("id");

                // Check if this is deleting an existing item
                if (moddedItem.GetBool("deleted"))
                {
                    // Try to find an item with this ID
                    int foundIndex = -1;
                    for (int i = 0; i < items.Count; i++)
                        if (((Hashtable)items[i])["id"].ToString() == moddedItemId)
                        {
                            foundIndex = i;
                            break;
                        }
                    if (foundIndex < 0)
                        NoonUtility.Log(
                            "Tried to delete '" + moddedItemId + "' but was not found",
                            messageLevel: 1);
                    else
                    {
                        NoonUtility.Log("Deleted '" + moddedItemId + "'");
                        items.RemoveAt(foundIndex);
                    }

                    continue;
                }

                Hashtable originalItem = null;
                var parents = new Dictionary<string, Hashtable>();
                var parentsOrder = moddedItem.GetArrayList("extends") ?? new ArrayList();
                foreach (Hashtable item in items)
                {
                    // Check if this item is overwriting an existing item (this will consider only the first matching
                    // item - normally, there should only be one)
                    var itemId = item.GetString("id");
                    if (itemId == moddedItemId && originalItem == null)
                    {
                        originalItem = item;
                    }

                    // Collect all the parents of this modded item so that the full item can be built
                    if (parentsOrder.Contains(itemId))
                    {
                        parents[itemId] = item;
                    }
                }

                // Build the new item, first by copying its parents, then by applying its own specificities
                // If the new item should override an older one, replace that one too
                var newItem = new Hashtable();
                foreach (string parent in parentsOrder)
                {
                    if (!parents.ContainsKey(parent))
                    {
                        NoonUtility.Log(
                            "Unknown parent '" + parent + "' for '" + moddedItemId + "', skipping parent",
                            messageLevel: 2);
                        continue;
                    }
                    newItem.AddHashtable(parents[parent], false);
                }
                newItem.AddHashtable(moddedItem, true);

                // Run any property operations that are present
                ProcessPropertyOperations(newItem);

                if (originalItem != null)
                {
                    originalItem.Clear();
                    originalItem.AddHashtable(newItem, true);
                }
                else
                {
                    items.Add(newItem);
                }
            }
            return items;
        }

        private static void ProcessPropertyOperations(Hashtable item)
        {
            var itemId = item.GetString("id");
            var keys = new ArrayList(item.Keys);
            foreach (string property in keys)
            {
                var propertyWithOperation = property.Split('$');
                if (propertyWithOperation.Length < 2)
                {
                    continue;
                }
                if (propertyWithOperation.Length > 2)
                {
                    NoonUtility.Log(
                        "Property '" + property + "' in '" + itemId + "' contains too many '$', skipping", messageLevel: 1);
                    continue;
                }

                var originalProperty = propertyWithOperation[0];
                if (!item.ContainsKey(originalProperty))
                {
                    NoonUtility.Log(
                        "Unknown property '" + originalProperty + "' for property '" + property + "' in '" + itemId + "', skipping",
                        messageLevel: 1);
                    continue;
                }
                var operation = propertyWithOperation[1];
                switch (operation)
                {
                    // append: append values to a list
                    // prepend: prepend values to a list
                    case "append":
                    case "prepend":
                        {
                            var value = item.GetArrayList(originalProperty);
                            var newValue = item.GetArrayList(property);
                            if (value == null || newValue == null)
                            {
                                NoonUtility.Log(
                                    "Cannot apply '{operation}' to '" + originalProperty + "' in '" + itemId + "': invalid type, must be a list",
                                    messageLevel: 1);
                                continue;
                            }

                            if (operation == "append")
                            {
                                value.AddRange(newValue);
                            }
                            else
                            {
                                value.InsertRange(0, newValue);
                            }

                            break;
                        }

                    // plus: Adds a numerical value to another.
                    // minus: Subtracts a numerical value from another.
                    case "plus":
                    case "minus":
                        {
                            var value = item.GetFloat(originalProperty);
                            var newValue = item.GetFloat(property);

                            var modifier = operation == "plus" ? 1 : -1;
                            item[originalProperty] = value + newValue * modifier;
                            break;
                        }

                    // extend: add or replace keys in a dictionary
                    case "extend":
                        {
                            var value = item.GetHashtable(originalProperty);
                            var newValue = item.GetHashtable(property);
                            if (value == null || newValue == null)
                            {
                                NoonUtility.Log(
                                    "Cannot apply '{operation}' to '" + originalProperty + "' in '" + itemId + "': invalid type, must be a dictionary",
                                    messageLevel: 1);
                                continue;
                            }

                            value.AddHashtable(newValue, true);

                            break;
                        }

                    // remove: removes items from a dictionary or a list
                    case "remove":
                        {
                            var newValue = item.GetArrayList(property);
                            if (newValue == null)
                            {
                                NoonUtility.Log(
                                    "Invalid value for '" + property + "' in '" + itemId + "': invalid type, must be a list",
                                    messageLevel: 1);
                                continue;
                            }

                            if (!item.ContainsKey(originalProperty))
                            {
                                NoonUtility.Log(
                                    "Cannot apply '{operation}' to '" + originalProperty + "' in '" + itemId + "': failed to find '" + originalProperty + "'",
                                    messageLevel: 1);
                                continue;
                            }

                            object originalPropertyValue = item[originalProperty];
                            if (originalPropertyValue.GetType() == typeof(Hashtable))
                            {
                                var value = item.GetHashtable(originalProperty);
                                foreach (string toDelete in newValue)
                                {
                                    if (value.ContainsKey(toDelete))
                                        value.Remove(toDelete);
                                    else
                                        NoonUtility.Log(
                                            "Failed to delete '" + toDelete + "' from '" + originalProperty + "' in '" + itemId + "'",
                                            messageLevel: 1);
                                }
                            }
                            else if (originalPropertyValue.GetType() == typeof(ArrayList))
                            {
                                var value = item.GetArrayList(originalProperty);
                                foreach (string toDelete in newValue)
                                {
                                    if (value.Contains(toDelete))
                                        value.Remove(toDelete);
                                    else
                                        NoonUtility.Log(
                                            "Failed to delete '" + toDelete + "' from '" + originalProperty + "' in '" + itemId + "'",
                                            messageLevel: 1);
                                }
                            }
                            else
                            {
                                NoonUtility.Log(
                                    "Cannot apply '{operation}' to '" + originalProperty + "' in '" + itemId + "': invalid type, must be a dictionary or a list",
                                    messageLevel: 1);
                            }

                            break;
                        }
                    default:
                        NoonUtility.Log(
                            "Unknown operation '{operation}' for property '" + property + "' in '" + itemId + "', skipping",
                            messageLevel: 1);
                        continue;
                }

                // Remove the property once it has been processed, to avoid warnings from the content importer
                item.Remove(property);
            }
        }

        public bool CopyFields(ArrayList dest, ArrayList src, string[] fieldsToTranslate, bool forceTranslate, bool autorepair, ref bool changedDst)
        {
            // Every field in dest that matches one of the names in fieldsToTranslate should be replaced by the equivalent field from src

            // First : Validation!
            if (dest == null || src == null)
            {
                return false;
            }
            if (dest.Count != src.Count)
            {
                NoonUtility.Log("Native entries=" + dest.Count + " but loc entries=" + src.Count, messageLevel: 1);
                changedDst = true;      // Force a reexport
            }
            if (dest.Count == 0 || src.Count == 0)
            {
                return false;
            }

            // Split the indexing in order to try to step over missing/extra entries to get back in sync, but actually...
            // ...REQUIRING the loc data to remain in sync is the only reliable way to detect errors.
            int srcIdx = 0;
            int destIdx = 0;

            do
            {
                if (srcIdx < src.Count)
                {
                    Debug.Assert(src[srcIdx].GetType() == dest[destIdx].GetType(), "Type mismatch in JSON original vs. translated");

                    bool localForceTranslate = forceTranslate;
                    for (int j = 0; j < fieldsToTranslate.Length; j++)
                    {
                        if (dest[destIdx].ToString().Equals(fieldsToTranslate[j]))
                        {
                            localForceTranslate = true;
                        }
                    }

                    if (dest[destIdx].GetType() == typeof(ArrayList))
                    {
                        CopyFields(dest[destIdx] as ArrayList, src[srcIdx] as ArrayList, fieldsToTranslate, localForceTranslate, autorepair, ref changedDst);
                    }
                    else if (dest[destIdx].GetType() == typeof(Hashtable))
                    {
                        if (!CopyFields(dest[destIdx] as Hashtable, src[srcIdx] as Hashtable, fieldsToTranslate, localForceTranslate, autorepair, ref changedDst))
                        {
                            // Auto-repair loc data
                            if (autorepair)
                            {
                                Hashtable hash = dest[destIdx] as Hashtable;
                                if (hash != null)
                                {
                                    hash.Add("comment", "NEW");
                                    changedDst = true;
                                    srcIdx--;   // Step back so that the increment at end of loop leaves us in the same place
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Auto-repair loc data
                    if (autorepair)
                    {
                        Hashtable hash = dest[destIdx] as Hashtable;
                        if (hash != null)
                        {
                            hash.Add("comment", "NEW");
                            changedDst = true;
                        }
                    }
                }

                srcIdx++;
                destIdx++;
            }
            while (destIdx < dest.Count);

            return true;
        }

        private bool CopyFields(Hashtable dest, Hashtable src, string[] fieldsToTranslate, bool forceTranslate, bool autorepair, ref bool changedDst)
        {
            // Copy localised language feilds from one dataset to the other
            // Every field in dest that matches one of the names in fieldsToTranslate should be replaced by the equivalent field from src

            if (dest == null || src == null)
            {
                return false;
            }

            // First : Validation!
            if (dest["id"] != null && src["id"] != null)
            {
                if (dest["id"].MakeString().CompareTo(src["id"].MakeString()) != 0)
                {
                    NoonUtility.Log("ERROR: Localisation expected [" + dest["id"].MakeString() + "] but found [" + src["id"].MakeString() + "]", messageLevel: 2);

                    //Debug.LogWarning("JSON original and translated don't match!");
                    return false;
                }
            }

            //Debug.Log("Localising ["+ dest["id"].MakeString() +"]");		// Commented out to keep log clear for now

            // Prep array lists so we can iterate in sync
            ArrayList destList = new ArrayList(dest.Values);
            //ArrayList srcList = new ArrayList(src.Values); commented out: not in use. - AK
            ArrayList destKeys = new ArrayList(dest.Keys);

            if (!forceTranslate)
            {
                // Check for our fields of interest
                for (int j = 0; j < fieldsToTranslate.Length; j++)
                {
                    if (src.ContainsKey(fieldsToTranslate[j]))
                    {
                        var source = src[fieldsToTranslate[j]];
                        if (source.GetType() == typeof(Hashtable))
                        {
                            CopyFields((Hashtable)dest[fieldsToTranslate[j]], (Hashtable)source, new string[] { }, true, autorepair, ref changedDst);
                        }
                        else
                        {
                            dest[fieldsToTranslate[j]] = source.ToString();
                        }
                    }
                    else if (dest.ContainsKey(fieldsToTranslate[j]))
                    {
                        if (dest.GetValue(fieldsToTranslate[j]).ToString().Length > 0)
                        {
                            // Error if src is missing a field that is present in the native JSON and has content
                            Debug.LogWarning("Missing loc field [" + fieldsToTranslate[j] + "] in set [" + dest["id"].MakeString() + "]");
                        }
                    }
                }
            }

            // Now recurse into any nested lists
            for (int i = 0; i < destList.Count; i++)
            {
                bool localForceTranslate = forceTranslate;
                for (int j = 0; j < fieldsToTranslate.Length; j++)
                {
                    if (destKeys[i].ToString().Equals(fieldsToTranslate[j]))
                    {
                        localForceTranslate = true;
                    }
                }

                if (destList[i].GetType() == typeof(ArrayList))
                {
                    CopyFields(dest[destKeys[i]] as ArrayList, src[destKeys[i]] as ArrayList, fieldsToTranslate, localForceTranslate, autorepair, ref changedDst);
                }
                else if (destList[i].GetType() == typeof(Hashtable))
                {
                    CopyFields(dest[destKeys[i]] as Hashtable, src[destKeys[i]] as Hashtable, fieldsToTranslate, localForceTranslate, autorepair, ref changedDst);
                }
                else if (forceTranslate)
                {
                    // Translate all fields except "id"
                    if (destKeys[i].ToString().Equals("id") == false)
                    {
                        dest[destKeys[i]] = src[destKeys[i]].ToString();
                    }
                }
            }
            return true;
        }

     public   void Export(string fname, string contentType, ArrayList list)
        {
            int indent = 0;
            StreamWriter writer = new StreamWriter(fname, false, System.Text.Encoding.UTF8);
            writer.WriteLine("{\n" + contentType + ": [");
            ExportRecurse(writer, list, ref indent);
            writer.WriteLine("]\n}");
            writer.Close();
        }

        void ExportRecurse(StreamWriter writer, ArrayList list, ref int indent)
        {
            indent++;
            string margin = "";
            for (int n = 0; n < indent; n++)
            {
                margin += "\t";
            }

            int i = 0;
            do
            {
                if (list[i].GetType() == typeof(ArrayList))
                {
                    writer.WriteLine(margin + "[");
                    ExportRecurse(writer, list[i] as ArrayList, ref indent);
                    writer.WriteLine(margin + "],");
                }
                else if (list[i].GetType() == typeof(Hashtable))
                {
                    writer.WriteLine(margin + "{");
                    ExportRecurse(writer, list[i] as Hashtable, ref indent);
                    writer.WriteLine(margin + "},");
                }
                else
                {
                    writer.WriteLine(margin + "\t" + list[i].ToString() + ",");
                }

                i++;
            }
            while (i < list.Count);

            indent--;
        }

        void ExportRecurse(StreamWriter writer, Hashtable ht, ref int indent)
        {
            indent++;
            string margin = "";
            for (int n = 0; n < indent; n++)
            {
                margin += "\t";
            }

            foreach (DictionaryEntry item in ht)
            {
                if (item.Value.GetType() == typeof(ArrayList))
                {
                    writer.WriteLine(margin + item.Key + ": ");
                    writer.WriteLine(margin + "[");
                    ExportRecurse(writer, item.Value as ArrayList, ref indent);
                    writer.WriteLine(margin + "],");
                }
                else if (item.Value.GetType() == typeof(Hashtable))
                {
                    writer.WriteLine(margin + item.Key + ": ");
                    writer.WriteLine(margin + "{");
                    ExportRecurse(writer, item.Value as Hashtable, ref indent);
                    writer.WriteLine(margin + "},");
                }
                else if (item.Value.GetType() == typeof(string))
                {
                    writer.WriteLine(margin + item.Key + ": \"" + item.Value + "\",");
                }
                else
                {
                    writer.WriteLine(margin + item.Key + ": " + item.Value + ",");
                }
            }

            indent--;
        }
    }
}
