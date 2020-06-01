
//#define LOC_AUTO_REPAIR		// Useful for importing fresh loc data into game. Merges current data with localised strings and outputs partially localised files with new data annotated - CP
								// NB. Running autorepair repeatedly will flush the "NEW" comments out because it modifies the source data in-place,
								// so on the second run the added hashtables are not considered new.
								// Enable this #define...run ONCE on target language, then turn it off again to test the autorepaired data.

using UnityEngine;
using System;
using Noon;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
#if MODS
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
#endif
using OrbCreationExtensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ContentImporter
{
    private IList<ContentImportMessage> contentImportMessages;
    private const string CONST_CONTENTDIR = "content/";
    private static readonly string CORE_CONTENT_DIR = Application.streamingAssetsPath + "/content/core/";
    private static readonly string MORE_CONTENT_DIR = Application.streamingAssetsPath + "/content/more/";
    private const string CONST_ELEMENTS = "elements";
    private const string CONST_RECIPES = "recipes";
    private const string CONST_VERBS = "verbs";
    private const string CONST_DECKS = "decks";
    private const string CONST_LEGACIES = "legacies";
    private const string CONST_ENDINGS = "endings";
    public ICompendium _compendium { get; private set; }


    public Dictionary<string, IVerb> Verbs;
    public Dictionary<string, Element> Elements;
    public Dictionary<string, Legacy> Legacies;
    public Dictionary<string, Ending> Endings;
    public List<Recipe> Recipes;
    private Dictionary<string, IDeckSpec> DeckSpecs;
    
    private static readonly Regex DlcLegacyRegex = new Regex(@"DLC_(\w+)_\w+_legacy\.json");


    public ContentImporter()
    {
        contentImportMessages = new List<ContentImportMessage>();
        Verbs = new Dictionary<string, IVerb>();
        Elements = new Dictionary<string, Element>();
        Recipes = new List<Recipe>();
        DeckSpecs = new Dictionary<string, IDeckSpec>();
        Legacies = new Dictionary<string, Legacy>();
		Endings = new Dictionary<string, Ending>();
    }

    public static IEnumerable<string> GetInstalledDlc()
    {
        return from path in Directory.GetFiles(Path.Combine(CORE_CONTENT_DIR, CONST_LEGACIES)) 
            select Path.GetFileName(path) into fileName 
            select DlcLegacyRegex.Match(fileName) into match 
            where match.Success 
            select match.Groups[1].Value;
    }

    private void LogProblem(string problemDesc)
    {
        contentImportMessages.Add(new ContentImportMessage(problemDesc));
    }

    private void LogInfo(string desc)
    {
        contentImportMessages.Add(new ContentImportMessage(desc,0));
    }


    public List<SlotSpecification> AddSlotsFromArrayList(ArrayList alSlots)
    {
        List<SlotSpecification> cssList = new List<SlotSpecification>();


            foreach (Hashtable htThisSlot in alSlots)
            {
                string slotId = htThisSlot[NoonConstants.KID].ToString();

                SlotSpecification slotSpecification = new SlotSpecification(slotId);
                try
                {
                    if (htThisSlot[NoonConstants.KLABEL] != null)
                        slotSpecification.Label = htThisSlot[NoonConstants.KLABEL].ToString();

                if (htThisSlot[NoonConstants.KDESCRIPTION] != null)
                        slotSpecification.Description = htThisSlot[NoonConstants.KDESCRIPTION].ToString();

                    if ((string) htThisSlot[NoonConstants.KGREEDY] == "true")
                        slotSpecification.Greedy = true;

                    if ((string) htThisSlot[NoonConstants.KCONSUMES] == "true")
                        slotSpecification.Consumes = true;


                    if ((string)htThisSlot[NoonConstants.KNOANIM] == "true")
                        slotSpecification.NoAnim = true;

                if (htThisSlot[NoonConstants.KACTIONID] != null)
                        slotSpecification.ForVerb = htThisSlot[NoonConstants.KACTIONID].ToString();


                Hashtable htRequired = htThisSlot[NoonConstants.KREQUIRED] as Hashtable;
                    if (htRequired != null)
                    {
                        foreach (string rk in htRequired.Keys)
                            slotSpecification.Required.Add(rk, Convert.ToInt32(htRequired[rk]));
                    }

                    Hashtable htForbidden = htThisSlot[NoonConstants.KFORBIDDEN] as Hashtable;
                    if (htForbidden != null)
                    {
                        foreach (string fk in htForbidden.Keys)
                            slotSpecification.Forbidden.Add(fk, Convert.ToInt32(htRequired[fk]));
                    }
                }
                catch (Exception e)
                {
                    LogProblem("Couldn't retrieve slot " + slotId + " - " + e.Message);
                }

                cssList.Add(slotSpecification);
            }


        return cssList;

    }

    private ArrayList GetContentItems(string contentOfType)
    {
        var contentFolder = CORE_CONTENT_DIR + contentOfType;
        var contentOverrideFolder = MORE_CONTENT_DIR + contentOfType;
        var coreContentFiles = Directory.GetFiles(contentFolder).ToList().FindAll(f => f.EndsWith(".json"));
        if(coreContentFiles.Any())
          coreContentFiles.Sort();
        var overridecontentFiles = Directory.GetFiles(contentOverrideFolder).ToList().FindAll(f => f.EndsWith(".json"));
        if(overridecontentFiles.Any())
             overridecontentFiles.Sort();

        List<string> allContentFiles = new List<string>();

        allContentFiles.AddRange(coreContentFiles);
        allContentFiles.AddRange(overridecontentFiles);
        if (!allContentFiles.Any())
            LogProblem("Can't find any " + contentOfType + " to import as content");

        ArrayList contentItemArrayList = new ArrayList();
		ArrayList originalArrayList = new ArrayList();
        ArrayList localisedArrayList = new ArrayList();
       allContentFiles.Sort();
        foreach (var contentFile in allContentFiles)
        {
            string json = File.ReadAllText(contentFile);
            try
            {
                originalArrayList = SimpleJsonImporter.Import(json).GetArrayList(contentOfType);
            }
            catch (Exception e)
            {
                LogProblem("This file broke: " + contentFile + " with error " + e.Message);
                return null;
            }

			// Now look for localised language equivalent of the same file and parse that
			string locFolder = "core_" + LanguageTable.targetCulture; //ahem. - AK
			string locFile = contentFile;
			locFile = locFile.Replace( "core", locFolder ); //ahem, further. - AK
            if (File.Exists(locFile))	// If no file exists, no localisation happens
			{
				json = File.ReadAllText(locFile);
				if (json.Length > 0)
				{
					try
					{
						localisedArrayList = SimpleJsonImporter.Import(json,true).GetArrayList(contentOfType);
					}
					catch (Exception e)
					{
						LogProblem("This file broke: " + contentFile + " with error " + e.Message);
                        return null;
					}



					bool repair = false;
					bool changed = false;
#if UNITY_EDITOR && LOC_AUTO_REPAIR
					//if (locFile.EndsWith("events.json"))
						repair = true;
#endif
					// We now have two sets of data which SHOULD match pair for pair - english and translated.
					// Traverse the dataset copying the following fields into the core data. Add new fields here if they need translating.
					// If the field is a list it will have ALL contents inside localised
					string[] fieldsToTranslate = { "label", "description", "startdescription", "drawmessages" };

					//
					// COPY LOCALISATION DATA INTO originalArrayList
					//
					CopyFields( originalArrayList, localisedArrayList, fieldsToTranslate, false, repair, ref changed );

#if UNITY_EDITOR && LOC_AUTO_REPAIR
NoonUtility.Log("Localising ["+ locFile +"]");  //AK: I think this should be here?
    //(a) we don't actually autofix the file unless one is missing, and
    //(b) the log is currently showing messages about the /more files, which shouldn't be localised to /core anyway.
					if (changed)
					{
						bool testOutput = false;
						if (testOutput)
						{
							/*
							string backupFile = locFile.Replace( ".json", "_backup.json" );
							if (!File.Exists(backupFile))
							{
								FileUtil.CopyFileOrDirectory(locFile,backupFile);	// Soft backup - skip if already there
							}
							*/
							string outputFile = locFile.Replace( ".json", "_out.json" );
							Export( outputFile, contentOfType, originalArrayList );
							//FileUtil.ReplaceFile(outputFile,locFile);			// Hard replace
						}
						else
						{
							Export( locFile, contentOfType, originalArrayList );
						}
						NoonUtility.Log("Exported ["+ locFile +"]");
					}
#endif
                }
            }

			contentItemArrayList.AddRange( originalArrayList );
        }
#if MODS
        return ProcessContentItemsWithMods(contentItemArrayList, contentOfType);
#else
        return contentItemArrayList;
#endif
    }

#if MODS
    private ArrayList ProcessContentItemsWithMods(ArrayList items, string contentOfType)
    {
        var modManager = Registry.Retrieve<ModManager>();
            var moddedItems = modManager.GetContentForCategory(contentOfType);
            foreach (var moddedItem in moddedItems)
            {
                var moddedItemId = moddedItem.GetString("id");

                // Check if this is deleting an existing item
                if (moddedItem.GetBool("deleted"))
                {
                    // Try to find an item with this ID
                    int foundIndex = -1;
                    for (int i = 0; i < items.Count; i++)
                        if (((Hashtable) items[i])["id"].ToString() == moddedItemId)
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
#endif

	private bool CopyFields( ArrayList dest, ArrayList src, string[] fieldsToTranslate, bool forceTranslate, bool autorepair, ref bool changedDst )
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
			changedDst = true;		// Force a reexport
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
			if (srcIdx<src.Count)
			{
				Debug.Assert(src[srcIdx].GetType()==dest[destIdx].GetType(), "Type mismatch in JSON original vs. translated");

				bool localForceTranslate = forceTranslate;
				for (int j=0; j<fieldsToTranslate.Length; j++)
				{
					if (dest[destIdx].ToString().Equals(fieldsToTranslate[j]))
					{
						localForceTranslate = true;
					}
				}

				if (dest[destIdx].GetType()==typeof(ArrayList))
				{
					CopyFields( dest[destIdx] as ArrayList, src[srcIdx] as ArrayList, fieldsToTranslate, localForceTranslate, autorepair, ref changedDst );
				}
				else if (dest[destIdx].GetType()==typeof(Hashtable))
				{
					if (!CopyFields( dest[destIdx] as Hashtable, src[srcIdx] as Hashtable, fieldsToTranslate, localForceTranslate, autorepair, ref changedDst ))
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
		while (destIdx<dest.Count);

		return true;
	}

	private bool CopyFields( Hashtable dest, Hashtable src, string[] fieldsToTranslate, bool forceTranslate, bool autorepair, ref bool changedDst )
	{
		// Copy localised language feilds from one dataset to the other
		// Every field in dest that matches one of the names in fieldsToTranslate should be replaced by the equivalent field from src

		if (dest == null || src == null)
		{
			return false;
		}

		// First : Validation!
		if (dest["id"] != null && src["id"]!=null)
		{
			if (dest["id"].MakeString().CompareTo( src["id"].MakeString() ) != 0)
			{
				NoonUtility.Log("ERROR: Localisation expected ["+ dest["id"].MakeString() +"] but found ["+ src["id"].MakeString() +"]", messageLevel: 2);

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
			for (int j=0; j<fieldsToTranslate.Length; j++)
			{
				if (src.ContainsKey(fieldsToTranslate[j]))
                {
                    var source = src[fieldsToTranslate[j]];
                    if (source.GetType() == typeof(Hashtable))
                    {
                        CopyFields((Hashtable) dest[fieldsToTranslate[j]], (Hashtable) source, new string[] {}, true, autorepair, ref changedDst);
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
						Debug.LogWarning("Missing loc field ["+ fieldsToTranslate[j] +"] in set ["+ dest["id"].MakeString() +"]");
					}
				}
			}
		}

		// Now recurse into any nested lists
		for (int i=0; i<destList.Count; i++)
        {
			bool localForceTranslate = forceTranslate;
			for (int j=0; j<fieldsToTranslate.Length; j++)
			{
				if (destKeys[i].ToString().Equals(fieldsToTranslate[j]))
				{
					localForceTranslate = true;
				}
			}

			if (destList[i].GetType()==typeof(ArrayList))
			{
				CopyFields( dest[destKeys[i]] as ArrayList, src[destKeys[i]] as ArrayList, fieldsToTranslate, localForceTranslate, autorepair, ref changedDst );
			}
			else if (destList[i].GetType()==typeof(Hashtable))
			{
				CopyFields( dest[destKeys[i]] as Hashtable, src[destKeys[i]] as Hashtable, fieldsToTranslate, localForceTranslate, autorepair, ref changedDst );
			}
			else if (forceTranslate)
			{
				// Translate all fields except "id"
				if (destKeys[i].ToString().Equals( "id" ) == false)
				{
					dest[destKeys[i]] = src[destKeys[i]].ToString();
				}
			}
		}
		return true;
	}

	void Export( string fname, string contentType, ArrayList list )
	{
		int indent = 0;
		StreamWriter writer = new StreamWriter(fname, false, System.Text.Encoding.UTF8);
        writer.WriteLine("{\n" + contentType + ": [");
		ExportRecurse( writer, list, ref indent );
		writer.WriteLine("]\n}");
		writer.Close();
	}

	void ExportRecurse( StreamWriter writer, ArrayList list, ref int indent )
	{
		indent++;
		string margin = "";
		for (int n=0; n<indent; n++)
		{
			margin += "\t";
		}

		int i = 0;
		do
		{
			if (list[i].GetType()==typeof(ArrayList))
			{
				writer.WriteLine( margin +"[" );
				ExportRecurse( writer, list[i] as ArrayList, ref indent );
				writer.WriteLine( margin +"]," );
			}
			else if (list[i].GetType()==typeof(Hashtable))
			{
				writer.WriteLine( margin +"{" );
				ExportRecurse( writer, list[i] as Hashtable, ref indent );
				writer.WriteLine( margin +"}," );
			}
			else
			{
				writer.WriteLine( margin + "\t" + list[i].ToString() + "," );
			}

			i++;
		}
		while (i<list.Count);

		indent--;
	}

	void ExportRecurse( StreamWriter writer, Hashtable ht, ref int indent )
	{
		indent++;
		string margin = "";
		for (int n=0; n<indent; n++)
		{
			margin += "\t";
		}

		foreach (DictionaryEntry item in ht)
		{
			if (item.Value.GetType() == typeof(ArrayList))
			{
				writer.WriteLine( margin + item.Key + ": " );
				writer.WriteLine( margin + "[" );
				ExportRecurse(writer, item.Value as ArrayList, ref indent);
				writer.WriteLine( margin + "]," );
			}
			else if (item.Value.GetType() == typeof(Hashtable))
			{
				writer.WriteLine( margin + item.Key + ": " );
				writer.WriteLine( margin + "{" );
				ExportRecurse(writer, item.Value as Hashtable, ref indent);
				writer.WriteLine( margin + "}," );
			}
			else if (item.Value.GetType() == typeof(string))
			{
				writer.WriteLine( margin + item.Key + ": \"" + item.Value + "\"," );
			}
			else
			{
				writer.WriteLine( margin + item.Key + ": " + item.Value + "," );
			}
		}

		indent--;
	}

    public void ImportElements(ArrayList alElements)
    {


        int totalElementsFound = 0;

        totalElementsFound += PopulateElements(alElements);

        LogInfo("Total elements found: " + totalElementsFound);

        foreach (var e in Elements)
        {
            foreach (var xt in e.Value.XTriggers)
            {
                foreach(var m in xt.Value)
                { 
                    if (!Elements.ContainsKey(m.Id))
                        LogProblem("Element " + e.Key + " specifies an invalid result (" + xt.Value + ") for xtrigger " + xt.Key);

                }
            }
            if(!string.IsNullOrEmpty(e.Value.DecayTo))
            {
                if(!Elements.ContainsKey(e.Value.DecayTo))
                    LogProblem("Element " + e.Key + " specifies an invalid result (" + e.Value.DecayTo + ") for DecayTo. ");

            }
        }
    }

    public int PopulateElements(ArrayList alElements)
    {

        if (alElements == null)
        {
            LogProblem("Elements were never imported; PopulateElements failed");
            return 0;
        }


        foreach (Hashtable htElement in alElements)
        {


            Hashtable htAspects = htElement.GetHashtable(NoonConstants.KASPECTS);
            ArrayList alSlots = htElement.GetArrayList(NoonConstants.KSLOTS);
            Hashtable htXTriggers = htElement.GetHashtable(NoonConstants.KXTRIGGERS);


            Element element = new Element(htElement.GetString(NoonConstants.KID),
              htElement.GetString(NoonConstants.KLABEL),
                htElement.GetString(NoonConstants.KDESCRIPTION),
                htElement.GetInt(NoonConstants.KANIMFRAMES),
                htElement.GetString(NoonConstants.KICON));

            try
            {

                    if (htElement.ContainsKey(NoonConstants.KLIFETIME))
                        element.Lifetime = float.Parse(htElement[NoonConstants.KLIFETIME].ToString());
                    if (htElement.ContainsKey(NoonConstants.KDECAYTO))
                        element.DecayTo = htElement.GetString(NoonConstants.KDECAYTO);

                    if (htElement.GetString(NoonConstants.KISASPECT) == "true")
                        element.IsAspect = true;
                    else
                        element.IsAspect = false;

                    if (htElement.GetString(NoonConstants.KISHIDDEN) == "true")
                        element.IsHidden = true;
                    else
                        element.IsHidden = false;

                    if (htElement.GetString(NoonConstants.KNOARTNEEDED) == "true")
                        element.NoArtNeeded = true;
                    else
                        element.NoArtNeeded = false;

                    if (htElement.GetString(NoonConstants.KRESATURATE) == "true")
                        element.Resaturate = true;
                    else
                        element.Resaturate = false;

                    if (htElement.GetString(NoonConstants.KUNIQUE) == "true")
                        element.Unique = true;
                    else
                        element.Unique = false;

                    element.OverrideVerbIcon = htElement.GetString(NoonConstants.KVERBOVERRIDEICON);

                    //INHERITANCE! only affects the properties below this line.

                    if (htElement.ContainsKey(NoonConstants.KINHERITS))
                    {
                        string inheritFromId = htElement.GetString(NoonConstants.KINHERITS);

                        if (!LogIfNonexistentElementId(inheritFromId, element.Id,
                            " (Trying to inherit from a nonexistent element)"))
                        {
                            Element inheritFromElement = Elements[inheritFromId];
                            element.InheritFrom(inheritFromElement);
                        }
                    }


                    element.Aspects.CombineAspects(NoonUtility.ReplaceConventionValues(htAspects)); //nb combine: we might have just inherited aspects

                    if (!string.IsNullOrEmpty(htElement.GetString(NoonConstants.KUNIQUENESSGROUP)))
                    {
                        element.UniquenessGroup = htElement.GetString(NoonConstants.KUNIQUENESSGROUP);
                        //and also... uniqueness groups are now also imported as aspects
                        //so this line needs to go below the assignment of aspects
                        element.Aspects.Add(element.UniquenessGroup,1);
                    }

                    if (alSlots!=null)
                        element.ChildSlotSpecifications = AddSlotsFromArrayList(alSlots);
                    foreach(var css in element.ChildSlotSpecifications)
                    {
                        if(string.IsNullOrEmpty(css.ForVerb))
                            LogProblem("No actionId for a slot on " + element.Id + " with id " + css.Id);
                    }
                    if (htXTriggers != null)
                    {
                        foreach (string k in htXTriggers.Keys)
                        {
                            if(htXTriggers[k].GetType()==typeof(ArrayList))
                            { 
                                ArrayList alMorphs = htXTriggers.GetArrayList(k);
                                if (alMorphs != null)
                                {
                                    List<MorphDetails> morphsForThisXTrigger=new List<MorphDetails>();
                                    foreach (Hashtable m in alMorphs)
                                    {
                                        string morphTo = m.GetString(NoonConstants.KID);
                                        int morphChance = Convert.ToInt32(m[NoonConstants.KCHANCE]);
                                        if (morphChance == 0)
                                            morphChance = 100; // if left at default, it's 100%

                                        int morphLevel = Convert.ToInt32(m[NoonConstants.KLEVEL]);
                                        if (morphLevel == 0)
                                            morphLevel = 1; // if left at default, it's 1


                                        MorphEffectType effectType=MorphEffectType.Transform;
                                        string possibleMorphEffect = m.GetString(NoonConstants.KMORPHEFFECT);
                                        if(!string.IsNullOrEmpty(possibleMorphEffect))
                                            try
                                            {
                                                effectType = (MorphEffectType)Enum.Parse(typeof(MorphEffectType), possibleMorphEffect, true);
                                            }
                                            catch
                                            {
                                                LogProblem(element.Id + " has a MorphEffect specified that we don't think is right: " +
                                                           possibleMorphEffect);
                                            }



                                        MorphDetails thisMorph= new MorphDetails(morphTo, morphChance,effectType,morphLevel);
                                        morphsForThisXTrigger.Add(thisMorph);
                                    }

                                    element.XTriggers.Add(k, morphsForThisXTrigger);
                                }
                            }
                            else if (htXTriggers[k] is string)
                            {
                                var xid = htXTriggers[k].ToString();
                                MorphDetails basicMorph = new MorphDetails(xid);
                                element.XTriggers.Add(k, new List<MorphDetails>{ basicMorph });
                            }
                            else
                            {
                                LogProblem("Can't process xtrigger " + k + " in " + element.Id);
                            }

                        }
                    
                    }

                    Elements.Add(element.Id, element);
            }
            catch (Exception e)
            {

                LogProblem("Couldn't add all properties for element " + element.Id + ": " + e.Message);

            }

            try
            {
                ArrayList alInducedRecipes = htElement.GetArrayList(NoonConstants.KINDUCES);
                if (alInducedRecipes != null)
                {
                    foreach (Hashtable ir in alInducedRecipes)
                    {
                        string lrID = ir[NoonConstants.KID].ToString();
                        int lrChance = Convert.ToInt32(ir[NoonConstants.KCHANCE]);
                        bool lrAdditional = Convert.ToBoolean(ir[NoonConstants.KADDITIONAL] ?? false);

                        var lrExpulsion = GetExpulsionDetailsIfAny(ir);


                        element.Induces.Add(new LinkedRecipeDetails(lrID, lrChance, lrAdditional, lrExpulsion,null));

                        if (lrChance == 0)
                        {
                            LogProblem("Chance 0 or not specified in induced recipes for element " + element.Id);
                        }

                        TryAddAsInternalRecipe(ir,null);
                    }
                }

            }
            catch (Exception e)
            {

                LogProblem("Problem importing induced recipes for element '" + element.Id + "' - " + e.Message);
            }
        }

        return alElements.Count;
    }

    public void ImportRecipes(ArrayList recipesArrayList)
    {
        //TextAsset[] recipeTextAssets = Resources.LoadAll<TextAsset>(CONST_CONTENTDIR + CONST_RECIPES);
       
        PopulateRecipeList(recipesArrayList);
        LogInfo("Total recipes found: " + recipesArrayList.Count);

    }

    public void ImportVerbs(ArrayList verbsArrayList)
    {

            //TextAsset[] verbTextAssets = Resources.LoadAll<TextAsset>(CONST_CONTENTDIR + CONST_VERBS);



        foreach (Hashtable h in verbsArrayList)
        {
            IVerb v = new BasicVerb(h["id"].ToString(), h["label"].ToString(), h["description"].ToString(),
                Convert.ToBoolean(h["atStart"]));
            ArrayList alSlots = h.GetArrayList(NoonConstants.KSLOTS);
            if (alSlots != null)
            {
        var slots=AddSlotsFromArrayList(alSlots);
                if (slots.Count > 1)
                    LogProblem(v.Id + " has more than one slot specified - we should only have a primary slot");
                else
                    v.PrimarySlotSpecification = slots.First();
            }

            
            Verbs.Add(v.Id, v);
        }

    }



    private void ImportDeckSpecs(ArrayList decksArrayList)
    {

        for (int i = 0; i < decksArrayList.Count; i++)
        {
            Hashtable htEachDeck = decksArrayList.GetHashtable(i);

            var d = PopulateDeckSpec(htEachDeck, htEachDeck["id"].ToString());

            DeckSpecs.Add(d.Id, d);
        }

    }

    private DeckSpec PopulateDeckSpec(Hashtable htEachDeck,string deckId)
    {
//deckspec
        var thisDeckSpec = new List<string>();
        try
        {
            ArrayList htDeckSpec = htEachDeck.GetArrayList(NoonConstants.KDECKSPEC);
            if (htDeckSpec != null)
            {
                foreach (string v in htDeckSpec)
                    thisDeckSpec.Add(v);
            }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing drawable items for deckSpec '" + htEachDeck[NoonConstants.KID].ToString() +
                       "' - " + e.Message);
        }


        bool resetOnExhaustion = false;
        try
        {
            resetOnExhaustion = Convert.ToBoolean(htEachDeck.GetValue(NoonConstants.KRESETONEXHAUSTION));
        }
        catch (Exception e)
        {
            LogProblem("Problem importing resetOnExhaustion  for deckSpec '" + htEachDeck[NoonConstants.KID].ToString() +
                       "' - " + e.Message);
        }

        string defaultCardId = "";
        //if we reset on exhaustion, we'll never see a default card, and we don't want
        //to throw an error on failing to import an unset default card.
        //Of course someone could have no default card and resetonexhaustion = false, but that's fundamentally their problem.
        if (!resetOnExhaustion)
            try
            {
                defaultCardId = htEachDeck.GetValue(NoonConstants.KDECKDEFAULTCARD).ToString();
            }
            catch (Exception e)
            {
                LogProblem("Problem importing default card for deckSpec '" + htEachDeck[NoonConstants.KID].ToString() +
                           "' - " + e.Message);
            }


        DeckSpec d = new DeckSpec(deckId, thisDeckSpec, defaultCardId, resetOnExhaustion);


        try
        {
            if (htEachDeck.GetValue(NoonConstants.KDECKDEFAULTDRAWS) != null)
            {
                var defaultDraws = Convert.ToInt32(htEachDeck.GetValue(NoonConstants.KDECKDEFAULTDRAWS));
                d.DefaultDraws = defaultDraws;
            }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing defaultDraws  for deckSpec '" + htEachDeck[NoonConstants.KID].ToString() +
                       "' - " + e.Message);
        }


        try
        {
            Hashtable htDrawMessages = htEachDeck.GetHashtable(NoonConstants.KDECKSPEC_DRAWMESSAGES);
            if (htDrawMessages != null)
            {
                d.DrawMessages = NoonUtility.HashtableToStringStringDictionary(htDrawMessages);

                foreach (var drawmessagekey in d.DrawMessages.Keys)
                {
                    if (!d.StartingCards.Contains(drawmessagekey))
                        LogProblem("Deckspec " + d.Id + " has a drawmessage for card " + drawmessagekey +
                                   ", but that card isn't in the list of drawable cards.");
                }
            }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing drawmessages for deckSpec '" + htEachDeck[NoonConstants.KID].ToString() +
                       "' - " + e.Message);
        }

        try
        {
            Hashtable htDefaultDrawMessages = htEachDeck.GetHashtable(NoonConstants.KDECKSPEC_DEFAULTDRAWMESSAGES);
            if (htDefaultDrawMessages != null)
            {
                d.DefaultDrawMessages = NoonUtility.HashtableToStringStringDictionary(htDefaultDrawMessages);
            }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing defaultdrawmessages for deckSpec '" + htEachDeck[NoonConstants.KID].ToString() +
                       "' - " + e.Message);
        }


        if (htEachDeck.ContainsKey(NoonConstants.KLABEL))
            d.Label = htEachDeck.GetValue(NoonConstants.KLABEL).ToString();
        if (htEachDeck.ContainsKey(NoonConstants.KDESCRIPTION))
            d.Description = htEachDeck.GetValue(NoonConstants.KDESCRIPTION).ToString();
        return d;
    }


    public void ImportLegacies(ArrayList legaciesArrayList)
    {

        for (int i = 0; i < legaciesArrayList.Count; i++)
        {
            Hashtable htEachLegacy = legaciesArrayList.GetHashtable(i);

            try
            {
                string legacyId = htEachLegacy[NoonConstants.KID].ToString();

                // Build the list of legacies to exclude, if provided
                // This should ONLY be applied to ending-dependent legacies, to avoid combining them with conflicting
                // randomly-selected legacies after an ending
                bool availableWithoutEndingMatch =
                    Convert.ToBoolean(htEachLegacy[NoonConstants.KAVAILABLEWITHOUTENDINGMATCH]);

                List<string> excludesOnEnding = new List<string>();
                if (htEachLegacy.ContainsKey(NoonConstants.KEXCLUDESONENDING))
                    excludesOnEnding = htEachLegacy.GetArrayList(NoonConstants.KEXCLUDESONENDING).Cast<string>().ToList();
                
                List<string> statusBarElements=new List<string>();
                if (htEachLegacy.Contains(NoonConstants.KSTATUSBARELEMENTS))
                    statusBarElements = htEachLegacy.GetArrayList(NoonConstants.KSTATUSBARELEMENTS).Cast<string>()
                        .ToList();

                string startingVerbId = string.Empty;
                if (htEachLegacy.ContainsKey(NoonConstants.KSTARTINGVERBID))
                    startingVerbId = htEachLegacy[NoonConstants.KSTARTINGVERBID].ToString();

                Legacy l = new Legacy(legacyId,
                    htEachLegacy[NoonConstants.KLABEL].ToString(),
                    htEachLegacy[NoonConstants.KDESCRIPTION].ToString(),
                    htEachLegacy[NoonConstants.KSTARTDESCRIPTION].ToString(),
                    htEachLegacy[NoonConstants.KIMAGE].ToString(),
                    htEachLegacy[NoonConstants.KFROMENDING].ToString(),
                    availableWithoutEndingMatch,
                    excludesOnEnding,
                    statusBarElements,
                    startingVerbId
                );

                Hashtable htEffects = htEachLegacy.GetHashtable(NoonConstants.KEFFECTS);
                if (htEffects != null)
                {
                    foreach (string k in htEffects.Keys)
                    {
                        LogIfNonexistentElementId(k, l.Id, "(effects)");
                        l.Effects.Add(k, Convert.ToInt32(htEffects[k]));
                    }
                }

                Legacies.Add(l.Id, l);
            }

            catch
            {
                LogProblem("Can't parse this legacy: " + htEachLegacy[NoonConstants.KID].ToString());
            }
        }


    }

    public void ImportEndings(ArrayList endingsArrayList)
    {

        for (int i = 0; i < endingsArrayList.Count; i++)
        {
            Hashtable htEachEnding = endingsArrayList.GetHashtable(i);

            try
            {
                string endingId = htEachEnding[NoonConstants.KID].ToString();

				EndingFlavour flavour = EndingFlavour.Melancholy;
				if (htEachEnding[NoonConstants.KFLAVOUR].ToString().CompareTo(EndingFlavour.Grand.ToString()) == 0)
					flavour = EndingFlavour.Grand;
				if (htEachEnding[NoonConstants.KFLAVOUR].ToString().CompareTo(EndingFlavour.Melancholy.ToString()) == 0)
					flavour = EndingFlavour.Melancholy;

                Ending end = new Ending(endingId,
                    htEachEnding[NoonConstants.KLABEL].ToString(),
                    htEachEnding[NoonConstants.KDESCRIPTION].ToString(),
                    htEachEnding[NoonConstants.KIMAGE].ToString(),
                    flavour,
                    htEachEnding[NoonConstants.KANIM].ToString(),
                    htEachEnding[NoonConstants.KACHIEVEMENT].ToString()
                );

                Endings.Add(endingId, end);
            }

            catch
            {
                LogProblem("Can't parse this legacy: " + htEachEnding[NoonConstants.KID].ToString());
            }
        }
    }


    public void PopulateRecipeList(ArrayList importedRecipes)
    {
        for (int i = 0; i < importedRecipes.Count; i++)
        {
            Hashtable htEachRecipe = importedRecipes.GetHashtable(i);

            ImportRecipe(htEachRecipe,null);
        }

       //check for common issues in recipes
        foreach (var r in Recipes)
        {
            if(r.Craftable && !r.Requirements.Any())
                LogProblem(r.Id + " is craftable, but has no requirements, so it will make its verb useless :O ");

            foreach (var n in r.LinkedRecipes)
                LogIfNonexistentRecipeId(n.Id, r.Id, " - as next recipe");
            foreach (var a in r.AlternativeRecipes)
                LogIfNonexistentRecipeId(a.Id, r.Id, " - as alternative");

            foreach (var m in r.MutationEffects)
            {
                LogIfNonexistentElementId(m.FilterOnAspectId,r.Id," - as mutation filter");
                LogIfNonexistentElementId(m.MutateAspectId, r.Id, " - as mutated aspect");
            }
        }
    }

    private void ImportRecipe(Hashtable htEachRecipe,string defaultActionId)
    {
        Recipe r = new Recipe();
        try
        {
            r.Id = htEachRecipe[NoonConstants.KID].ToString();
            htEachRecipe.Remove(NoonConstants.KID);

            r.Label = htEachRecipe[NoonConstants.KLABEL] == null
                ? r.Id
                : htEachRecipe[NoonConstants.KLABEL].ToString();
            htEachRecipe.Remove(NoonConstants.KLABEL);

            r.Craftable = Convert.ToBoolean(htEachRecipe[NoonConstants.KCRAFTABLE]);
            htEachRecipe.Remove(NoonConstants.KCRAFTABLE);

            r.SignalImportantLoop = Convert.ToBoolean(htEachRecipe[NoonConstants.KSIGNALIMPORTANTLOOP]);
            htEachRecipe.Remove(NoonConstants.KSIGNALIMPORTANTLOOP);

            if (htEachRecipe.Contains(NoonConstants.KPORTALEFFECT))
            {
                string possiblePortalEffect = htEachRecipe[NoonConstants.KPORTALEFFECT].ToString();
                try
                {
                    r.PortalEffect = (PortalEffect) Enum.Parse(typeof(PortalEffect), possiblePortalEffect, true);
                    htEachRecipe.Remove(NoonConstants.KPORTALEFFECT);
                }
                catch
                {
                    LogProblem(r.Id + " has a PortalEffect specified that we don't think is right: " +
                               possiblePortalEffect);
                }
            }

            r.HintOnly = Convert.ToBoolean(htEachRecipe[NoonConstants.KHINTONLY]);
            htEachRecipe.Remove(NoonConstants.KHINTONLY);

            r.ActionId = htEachRecipe[NoonConstants.KACTIONID] == null
                ? null
                : htEachRecipe[NoonConstants.KACTIONID].ToString();
            if (r.ActionId == null)
            {
                if (defaultActionId != null)
                    r.ActionId = defaultActionId;
                else
                    LogProblem(r.Id + " has no actionId specified");

            }
            htEachRecipe.Remove(NoonConstants.KACTIONID);

            if (htEachRecipe.ContainsKey(NoonConstants.KSTARTDESCRIPTION))
                r.StartDescription = htEachRecipe[NoonConstants.KSTARTDESCRIPTION].ToString();
            htEachRecipe.Remove(NoonConstants.KSTARTDESCRIPTION);


            if (htEachRecipe.ContainsKey(NoonConstants.KDESCRIPTION))
                r.Description = htEachRecipe[NoonConstants.KDESCRIPTION].ToString();
            htEachRecipe.Remove(NoonConstants.KDESCRIPTION);


            r.Warmup = Convert.ToInt32(htEachRecipe[NoonConstants.KWARMUP]);
            htEachRecipe.Remove(NoonConstants.KWARMUP);


            r.EndingFlag = htEachRecipe[NoonConstants.KENDING] == null
                ? null
                : htEachRecipe[NoonConstants.KENDING].ToString();
            htEachRecipe.Remove(NoonConstants.KENDING);

            if (htEachRecipe[NoonConstants.KSIGNALENDINGFLAVOUR] == null)

                r.SignalEndingFlavour = EndingFlavour.None;
            else
            {
                string possibleSignalEndingFlavour = htEachRecipe[NoonConstants.KSIGNALENDINGFLAVOUR].ToString();
                r.SignalEndingFlavour =
                    (EndingFlavour) Enum.Parse(typeof(EndingFlavour), possibleSignalEndingFlavour, true);
                htEachRecipe.Remove(NoonConstants.KSIGNALENDINGFLAVOUR);
            }


            if (htEachRecipe.ContainsKey(NoonConstants.KMAXEXECUTIONS))
                r.MaxExecutions = Convert.ToInt32(htEachRecipe[NoonConstants.KMAXEXECUTIONS]);
            htEachRecipe.Remove(NoonConstants.KMAXEXECUTIONS);

            if (htEachRecipe.ContainsKey(NoonConstants.KBURNIMAGE))
                r.BurnImage = htEachRecipe[NoonConstants.KBURNIMAGE].ToString();
            htEachRecipe.Remove(NoonConstants.KBURNIMAGE);
        }
        catch (Exception e)
        {
            string rawOutput = string.Empty;
            foreach (var v in htEachRecipe.Values)
                rawOutput = rawOutput + v + "||";

            if (htEachRecipe[NoonConstants.KID] == null)
            {
                LogProblem("Problem importing recipe with unknown id - " + rawOutput + e.Message);
            }
            else
            {
                LogProblem("Problem importing recipe '" + htEachRecipe[NoonConstants.KID] + "' - " + e.Message);
            }
        }

        //REQUIREMENTS
        try
        {
            Hashtable htReqs = htEachRecipe.GetHashtable(NoonConstants.KREQUIREMENTS);
            if (htReqs != null)
            {
                foreach (string k in htReqs.Keys)
                {
                    LogIfNonexistentElementId(k, r.Id, "(requirements)");
                    r.Requirements.Add(k, htReqs.GetString(k));
                }
            }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing requirements for recipe '" + r.Id + "' - " + e.Message);
        }

        //TABLE REQS

        htEachRecipe.Remove(NoonConstants.KREQUIREMENTS);

        try
        {
            Hashtable htTableReqs = htEachRecipe.GetHashtable(NoonConstants.KTABLEREQS);
            if (htTableReqs != null)
            {
                foreach (string k in htTableReqs.Keys)
                {
                    LogIfNonexistentElementId(k, r.Id, "(table requirements)");
                    r.TableReqs.Add(k, htTableReqs.GetString(k));
                }
            }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing table requirements for recipe '" + r.Id + "' - " + e.Message);
        }


        htEachRecipe.Remove(NoonConstants.KTABLEREQS);


        //extant REQS

        try
        {
            Hashtable htExtantReqs = htEachRecipe.GetHashtable(NoonConstants.KEXTANTREQS);
            if (htExtantReqs != null)
            {
                foreach (string k in htExtantReqs.Keys)
                {
                    LogIfNonexistentElementId(k, r.Id, "(extant requirements)");
                    r.ExtantReqs.Add(k, htExtantReqs.GetString(k));
                }
            }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing extant requirements for recipe '" + r.Id + "' - " + e.Message);
        }


        htEachRecipe.Remove(NoonConstants.KEXTANTREQS);

        /////////////////////////////////////////////

        //ASPECTS
        try
        {
            Hashtable htAspects = htEachRecipe.GetHashtable(NoonConstants.KASPECTS);
            if (htAspects != null)
            {
                foreach (string k in htAspects.Keys)
                {
                    LogIfNonexistentElementId(k, r.Id, "(aspects)");
                    r.Aspects.Add(k, Convert.ToInt32(htAspects[k]));
                }
            }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing aspects for recipe '" + r.Id + "' - " + e.Message);
        }

        htEachRecipe.Remove(NoonConstants.KASPECTS);

        /////////////////////////////////////////////


        //EFFECTS
        try
        {
            Hashtable htEffects = htEachRecipe.GetHashtable(NoonConstants.KEFFECTS);
            if (htEffects != null)
            {
                foreach (string k in htEffects.Keys)
                {
                    LogIfNonexistentElementId(k, r.Id, "(effects)");
                    r.Effects.Add(k, htEffects.GetString(k));
                }

            }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing effects for recipe '" + r.Id + "' - " + e.Message);
        }

        htEachRecipe.Remove(NoonConstants.KEFFECTS);


        /////////////////////////////////////////////
        //PURGES

        try
        {
            Hashtable htPurge = htEachRecipe.GetHashtable(NoonConstants.KPURGE);
            if(htPurge!=null)
                foreach (string elementToPurgeId in htPurge.Keys)
                {
                    LogIfNonexistentElementId(elementToPurgeId,r.Id,"purges");
                    r.Purge.Add(elementToPurgeId,Convert.ToInt32(htPurge[elementToPurgeId]));
                }
        }
        catch (Exception e)
        {
           LogProblem("Problem importing purges for recipe '" + r.Id + "' - " + e.Message);
        }

        htEachRecipe.Remove(NoonConstants.KPURGE);

        /////////////////////////////////////////////
        //VERB HALTS
        try
        {
            Hashtable htHaltVerb = htEachRecipe.GetHashtable(NoonConstants.KHALTVERB);
            if(htHaltVerb!=null)
                foreach (string verbToHaltId in (htHaltVerb.Keys))
                {
                    r.HaltVerb.Add(verbToHaltId,Convert.ToInt32(htHaltVerb[verbToHaltId]));
                }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing verb halts for recipe '" + r.Id + "' - " + e.Message);
        }

        htEachRecipe.Remove(NoonConstants.KHALTVERB);

        /////////////////////////////////////////////
        //VERB DELETIONS
        try
        {
            Hashtable htdeleteVerb = htEachRecipe.GetHashtable(NoonConstants.KDELETEVERB);
            if (htdeleteVerb != null)
                foreach (string verbTodeleteId in (htdeleteVerb.Keys))
                {
                    r.DeleteVerb.Add(verbTodeleteId, Convert.ToInt32(htdeleteVerb[verbTodeleteId]));
                }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing verb halts for recipe '" + r.Id + "' - " + e.Message);
        }

        htEachRecipe.Remove(NoonConstants.KDELETEVERB);

        /////////////////////////////////////////////
        //DECKS

        try
        {
            Hashtable htDecks = htEachRecipe.GetHashtable(NoonConstants.KDECKEFFECT);
            if (htDecks != null)
                foreach (string deckId in htDecks.Keys)
                {
                    LogIfNonexistentDeckId(deckId, r.Id);
                    r.DeckEffects.Add(deckId, Convert.ToInt32(htDecks[deckId]));
                }
        }

        catch (Exception e)
        {
            LogProblem("Problem importing decks for recipe '" + r.Id + "' - " + e.Message);
        }


        htEachRecipe.Remove(NoonConstants.KDECKEFFECT);





        ///////////INTERNAL DECKS - NB the deck is not stored with the recipe

        var htInternalDeck = htEachRecipe.GetHashtable(NoonConstants.KINTERNALDECK);
        if (htInternalDeck != null)
        {
            string internalDeckId = "deck." + r.Id;
            var internalDeck = PopulateDeckSpec(htInternalDeck, internalDeckId);
            r.DeckEffects.Add(internalDeckId, internalDeck.DefaultDraws);
            DeckSpecs.Add(internalDeckId, internalDeck);

            htEachRecipe.Remove(NoonConstants.KINTERNALDECK);
        }


        /////////////////////////////////////////////
        //SLOTS
        try
        {
            ArrayList alSlots = htEachRecipe.GetArrayList(NoonConstants.KSLOTS);
            if (alSlots != null)
                r.SlotSpecifications = AddSlotsFromArrayList(alSlots);
            if (r.SlotSpecifications.Count > 1)
                LogProblem(r.Id + " has more than one slot specified, which we don't allow at the moment.");
        }
        catch (Exception e)
        {
            LogProblem("Problem importing slots for recipe '" + r.Id + "' - " + e.Message);
        }

        htEachRecipe.Remove(NoonConstants.KSLOTS);
        /////////////////////////////////////////////
        //ALTERNATIVES
        try
        {
            ArrayList alRecipeAlternatives = htEachRecipe.GetArrayList(NoonConstants.KALTERNATIVERECIPES);
            if (alRecipeAlternatives == null)
                alRecipeAlternatives = htEachRecipe.GetArrayList(NoonConstants.KALTERNATIVERECIPESALT);
            if (alRecipeAlternatives != null)
            {
                foreach (Hashtable ra in alRecipeAlternatives)
                {
                    string raID = ra[NoonConstants.KID].ToString();
                    int raChance = Convert.ToInt32(ra[NoonConstants.KCHANCE]);
                    bool raAdditional = Convert.ToBoolean(ra[NoonConstants.KADDITIONAL] ?? false);

                    var raExpulsion = GetExpulsionDetailsIfAny(ra);

                    var htChallenges = ra.GetHashtable(NoonConstants.KCHALLENGES);

                    if (raChance == 0)
                    {
                        if (htChallenges == null)
                            raChance = 100;
                        else
                            raChance = 0;
                    }

                    r.AlternativeRecipes.Add(new LinkedRecipeDetails(raID, raChance, raAdditional, raExpulsion,
                        NoonUtility.HashtableToStringStringDictionary(htChallenges)));

                    TryAddAsInternalRecipe(ra,r);
                }
            }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing alternative recipes for recipe '" + r.Id + "' - " + e.Message);
        }

        htEachRecipe.Remove(NoonConstants.KALTERNATIVERECIPES);
        htEachRecipe.Remove(NoonConstants.KALTERNATIVERECIPESALT);


        try
        {
            ArrayList alLinkedRecipes = htEachRecipe.GetArrayList(NoonConstants.KLINKED);
            if (alLinkedRecipes != null)
            {
                foreach (Hashtable lr in alLinkedRecipes)
                {
                    string lrID = lr[NoonConstants.KID].ToString();
                    int lrChance = Convert.ToInt32(lr[NoonConstants.KCHANCE]);
                    bool lrAdditional = Convert.ToBoolean(lr[NoonConstants.KADDITIONAL] ?? false);

                    var lrExpulsion = GetExpulsionDetailsIfAny(lr);
                    var htChallenges = lr.GetHashtable(NoonConstants.KCHALLENGES);


                    if (lrChance == 0)
                    {
                        if (htChallenges == null)
                            lrChance = 100;
                        else
                            lrChance = 0;
                    }

                    r.LinkedRecipes.Add(new LinkedRecipeDetails(lrID, lrChance, lrAdditional, lrExpulsion,
                        NoonUtility.HashtableToStringStringDictionary(htChallenges)));

                    TryAddAsInternalRecipe(lr,r);
                }
            }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing linked recipes for recipe '" + r.Id + "' - " + e.Message);
        }

        htEachRecipe.Remove(NoonConstants.KLINKED);


        /////////////////////////////////////////////
        //MUTATIONEFFECTS
        try
        {
            ArrayList alMutations = htEachRecipe.GetArrayList(NoonConstants.KMUTATIONS);
            if (alMutations != null)
            {
                foreach (Hashtable htMutationEffect in alMutations)
                {
                    string filterOnAspectId = string.Empty;

                    if (htMutationEffect[NoonConstants.KMUTATIONLEVEL] != null)
                        filterOnAspectId = htMutationEffect[NoonConstants.KFILTERONASPECTID].ToString();
                    else if (htMutationEffect[NoonConstants.KFILTERONASPECTIDALT] != null)
                        filterOnAspectId = htMutationEffect[NoonConstants.KFILTERONASPECTIDALT].ToString();
                    else
                        LogProblem("Missing mutation filter specification for " + r.Id);


                    string mutateAspectId = string.Empty;

                    if (htMutationEffect[NoonConstants.KMUTATEASPECTID] != null)
                        mutateAspectId = htMutationEffect[NoonConstants.KMUTATEASPECTID].ToString();
                    else if (htMutationEffect[NoonConstants.KMUTATEASPECTIDALT] != null)
                        mutateAspectId = htMutationEffect[NoonConstants.KMUTATEASPECTIDALT].ToString();
                    else
                        LogProblem("Missing mutation specification for " + r.Id);


                    int mutationLevel = 0;

                    if (htMutationEffect[NoonConstants.KMUTATIONLEVEL] != null)
                        mutationLevel = Convert.ToInt32(htMutationEffect[NoonConstants.KMUTATIONLEVEL]);
                    else if (htMutationEffect[NoonConstants.KMUTATIONLEVELALT] != null)
                        mutationLevel = Convert.ToInt32(htMutationEffect[NoonConstants.KMUTATIONLEVELALT]);
                    else
                        LogProblem("Missing mutation level specification for " + r.Id);

                    bool additive = Convert.ToBoolean(htMutationEffect[NoonConstants.KADDITIVE] ?? false);

                    r.MutationEffects.Add(new MutationEffect(filterOnAspectId, mutateAspectId, mutationLevel, additive));
                }
            }
        }
        catch (Exception e)
        {
            LogProblem("Problem importing mutationEffects recipes for recipe '" + r.Id + "' - " + e.Message);
        }

        htEachRecipe.Remove(NoonConstants.KMUTATIONS);


        //Finished! Import, tidy up.
        Recipes.Add(r);

        htEachRecipe.Remove("comment"); //this should be the only nonprocessed property at this point
        htEachRecipe.Remove("comments"); //this should be the only nonprocessed property at this point

        foreach (var k in htEachRecipe.Keys)
        {
            LogProblem("Unprocessed recipe property for " + r.Id + ": " + k);
        }
    }

    private void TryAddAsInternalRecipe(Hashtable ra,Recipe wrappingRecipe)
    {
        ra.Remove(NoonConstants.KCHANCE);
        ra.Remove(NoonConstants.KADDITIONAL);
        ra.Remove(NoonConstants.KCHALLENGES);
        ra.Remove(NoonConstants.KEXPULSION);

        string possibleDefaultActionId = null;

        if (wrappingRecipe != null)
            possibleDefaultActionId = wrappingRecipe.ActionId;

        //internal recipe? can be specified inline, and then goes into the recipes list as standard
        if (ra.Count > 1) //for a non-internal recipe, ID is the only remaining property
            ImportRecipe(ra, possibleDefaultActionId);
    }

    private static Expulsion GetExpulsionDetailsIfAny(Hashtable linkedrecipedetails)
    {
        Expulsion possibleExpulsion = null;

        Hashtable htExpulsion = linkedrecipedetails.GetHashtable(NoonConstants.KEXPULSION);
        if (htExpulsion != null)
        {
            possibleExpulsion = new Expulsion();
            possibleExpulsion.Limit = htExpulsion.GetInt(NoonConstants.KLIMIT);
            Hashtable htFilter = htExpulsion.GetHashtable(NoonConstants.KFILTER);
            foreach (var k in htFilter.Keys)
            {
                possibleExpulsion.Filter.Add(k.ToString(), Convert.ToInt32(htFilter[k]));
            }
        }


        return possibleExpulsion;
    }

    private bool LogIfNonexistentElementId(string elementId, string containerId, string context)
    {
        if (!elementId.StartsWith(NoonConstants.LEVER_PREFIX) && !Elements.ContainsKey(elementId))
        { 
            LogProblem("'" + containerId + "' references non-existent element '" + elementId + "' " + " " + context);
            return true;
        }

        return false;
    }

    private void LogIfNonexistentDeckId(string deckId, string containerId)
    {
        if (!DeckSpecs.ContainsKey(deckId))
            LogProblem("'" + containerId + "' references non-existent deckSpec '" + deckId + "'");
    }

    private void LogIfNonexistentRecipeId(string referencedId, string parentRecipeId, string context)
    {
        if (referencedId != null && Recipes.All(r => r.Id != referencedId))
            LogProblem(
                "'" + parentRecipeId + "' references non-existent recipe '" + referencedId + "' " + " " + context);
    }

    private void LogMissingImages()
    {
        //check for missing images
        var allElements = _compendium.GetAllElementsAsDictionary();
        string missingAspectImages = "";
        int missingAspectImageCount = 0;
        string missingElementImages = "";
        int missingElementImageCount = 0;
        foreach (var k in allElements.Keys)
        {
           var thisElement = allElements[k];

            if (thisElement.IsAspect )
            {
                if ((!thisElement.NoArtNeeded && !thisElement.IsHidden) && (ResourcesManager.GetSpriteForAspect(thisElement.Icon) == null || ResourcesManager.GetSpriteForAspect(thisElement.Icon).name == ResourcesManager.PLACEHOLDER_IMAGE_NAME))
                {
                    missingAspectImages += (" " + k);
                    missingAspectImageCount++;
                }
            }
            else
            {


                if (!thisElement.NoArtNeeded && ResourcesManager.GetSpriteForElement(thisElement.Icon).name==ResourcesManager.PLACEHOLDER_IMAGE_NAME)
                {
                    missingElementImages += (" " + k);
                    missingElementImageCount++;
                }
            }
        }

        if (missingAspectImages != "")
            LogInfo("Missing " + missingAspectImageCount + " images for aspects:" + missingAspectImages);

        if (missingElementImages != "")
            LogInfo("Missing " + missingElementImageCount + " images for elephants:" + missingElementImages);
    }

    public IList<ContentImportMessage> PopulateCompendium(ICompendium compendium)
    {
        try
        {
            _compendium = compendium;
            ArrayList alVerbs = GetContentItems(CONST_VERBS);
            ArrayList alElements = GetContentItems(CONST_ELEMENTS);
            
            ArrayList alDeckSpecs = GetContentItems(CONST_DECKS);
            ArrayList alRecipes = GetContentItems(CONST_RECIPES);
            ArrayList alLegacies = GetContentItems(CONST_LEGACIES);
            ArrayList alEndings = GetContentItems(CONST_ENDINGS);

            if (contentImportMessages.Any(m=>m.MessageLevel>1))
                //at least one file is broken. Bug out and report.
                return contentImportMessages;

            ImportVerbs(alVerbs);
        ImportElements(alElements);
        ImportDeckSpecs(alDeckSpecs);
        ImportRecipes(alRecipes);
        ImportLegacies(alLegacies);
		ImportEndings(alEndings);



        //I'm not sure why I use fields rather than local variables returned from the import methods?
        //that might be something to tidy up; I suspect it's left from an early design

        _compendium.UpdateRecipes(Recipes);
        _compendium.UpdateElements(Elements);
        _compendium.UpdateVerbs(Verbs);
        _compendium.UpdateDeckSpecs(DeckSpecs);
        _compendium.UpdateLegacies(Legacies);
		_compendium.UpdateEndings(Endings);

foreach(var d in _compendium.GetAllDeckSpecs())
    d.RegisterUniquenessGroups(_compendium);


#if DEBUG
        CountWords();
        LogMissingImages();
        LogFnords();

        foreach (var kvp in DeckSpecs)
        {
            foreach (var c in kvp.Value.StartingCards)
            {
                if (!c.Contains(NoonConstants.DECK_PREFIX))
                    LogIfNonexistentElementId(c,kvp.Key, "(deckSpec spec items)");
            }
        }

#endif
        }
        catch (Exception e)
        {
            LogProblem(e.Message);
            return contentImportMessages;
        }

        return contentImportMessages;


    }

	private void OnLanguageChanged()
	{
	}

    private void CountWords()
    {
        int words = 0;
        foreach (var r in Recipes)
        {
            words += (r.Label.Count(char.IsWhiteSpace)+1);
            words += (r.StartDescription.Count(char.IsWhiteSpace) + 1);
            words += (r.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var e in Elements.Values)
        {

            words += (e.Label.Count(char.IsWhiteSpace) + 1);
            words += (e.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var v in Verbs.Values)
        {
            words += (v.Label.Count(char.IsWhiteSpace) + 1);
            words += (v.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var l in Legacies.Values)
        {
            words += (l.Label.Count(char.IsWhiteSpace) + 1);
            words += (l.StartDescription.Count(char.IsWhiteSpace) + 1);
            words += (l.Description.Count(char.IsWhiteSpace) + 1);
        }

       LogInfo("Words (based on spaces +1 count): " + words);

    }

    private void LogFnords()
    {
        const string FNORD = "FNORD";

        var allElements = _compendium.GetAllElementsAsDictionary();
        string elementFnords = "";
        int elementFnordCount = 0;
        foreach (var k in allElements.Keys)
        {
            var thisElement = allElements[k];

            if (thisElement.Label.ToUpper().Contains(FNORD)
            || thisElement.Description.ToUpper().Contains(FNORD)
            )
            {
                    elementFnords += (" " + k);
                    elementFnordCount++;
            }
        }

        var allRecipes = _compendium.GetAllRecipesAsList();
        string recipeFnords = "";
        int recipeFnordCount = 0;
        foreach (var r in allRecipes)
        {

            if (r.Label.ToUpper().Contains(FNORD)
                || r.StartDescription.ToUpper().Contains(FNORD)
                || r.Description.ToUpper().Contains(FNORD)

            )
            {

                recipeFnords += (" " + r.Id);
                recipeFnordCount++;
            }
        }


        if (elementFnords != "")
            LogInfo(elementFnordCount + "  fnords for elements:" + elementFnords);

        if (recipeFnords != "")
            LogInfo(recipeFnordCount + "  fnords for recipes:" + recipeFnords);


    }

}
