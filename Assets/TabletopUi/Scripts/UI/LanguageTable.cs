using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

class LocTriplet
{
	public LocTriplet( string _id, string _data )
	{
		id = _id;
		data = _data.Replace( "\\n", "\n" );	// Have to fix double escape for every string just in case :/
		hash = Animator.StringToHash( _id );
	}

	public bool Matches( int tgtHash )
	{
		return hash == tgtHash;
	}

	public bool Matches( string tgtId )
	{
		return id.Equals( tgtId );
	}

	public string GetId()
	{
		return id;
	}

	public string GetString()
	{
		return data;
	}

	private int hash;		// Hash code for identifer
	private string id;		// The identifier for the string, eg. "PAUSE_RESUME"
	private string data;	// The localised string itself, eg. "Resume"
};

public class LanguageTable : MonoBehaviour
{
	public static string csvFile = "content/strings.csv";
	public static string targetCulture = "en";

	private static List<LocTriplet> locTriplets;
	private static List<LocTriplet> cultures;

	public static string[] SplitCSVLine(string input)
	{
		// The standard string.Split(',') does not correctly handle commas inside quotes,
		// so I've made a bespoke function (courtesy of StackOverflow for the regex because OMG NO) 
		Regex csvSplit = new Regex("(?:^|,)(\"(?:([^\"]|\"\")*)*\"|[^,]*)");
		List<string> list = new List<string>();
		string curr = null;
		foreach (Match match in csvSplit.Matches(input))
		{        
			curr = match.Value;
			if (0 == curr.Length)
			{
				list.Add("");
			}

			list.Add(curr.TrimStart(',').TrimStart('"').TrimEnd('"'));
		}

		return list.ToArray();
	}

	public static void LoadCulture( string newTargetCulture )
	{
		targetCulture = newTargetCulture;

		if (csvFile != null)
		{
			string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, csvFile);
			string filetext = System.IO.File.ReadAllText(filePath, System.Text.Encoding.UTF8);		// Expects System.Text.Encoding.UTF8

#if UNITY_EDITOR
			Debug.Assert( filetext!=null, "Invalid CSV filename <" + filePath + ">!" );
			if (filetext == "")
			{
				Debug.LogError("Unable to read <" + filePath + "> - locked by another app?");
				Debug.Break();	// Prevent script running on if we hit this assert
			}
#endif
			if (locTriplets != null)
				locTriplets.Clear();
			else
				locTriplets = new List<LocTriplet>();

			if (cultures != null)
				cultures.Clear();
			else
				cultures = new List<LocTriplet>();
			
			if (filetext == null)	// Bad data? Early out after creating empy lists
				return;

			string[] lines = filetext.Split('\n');
			string[] entries = SplitCSVLine( lines[0] );

			// Populate internal list of languages available in the data
			Debug.Assert( entries.Length>=2, "Not enough columns for minimum ID and DATA!" );
			int cultureIndex = -1;
			for (int i=1; i<entries.Length; i++)
			{
				LocTriplet tri = new global::LocTriplet( entries[i], "CULTURE_" + entries[i] );
				cultures.Add( tri );

				if (string.CompareOrdinal(entries[i],targetCulture) == 0)
				{
					cultureIndex = i;
				}
			}
			Debug.Assert( cultureIndex>=1, "Unrecognised culture index!" );
			if (cultureIndex <= 0)
				return;

			for (int i=1; i<lines.Length; i++)
			{
				entries = SplitCSVLine( lines[i] );
				if (entries!=null && entries.Length>cultureIndex)
				{
					LocTriplet tri = new LocTriplet( entries[0], entries[cultureIndex] );
					//Debug.Log("Adding " + entries[0] + " <" + tri.GetHashCode() + "> " + entries[cultureIndex]);
					locTriplets.Add( tri );
				}
			}
			//Debug.Log( "Imported " + locTriplets.Count + " strings" );

			// TODO: sort into hash order so we can binary chop the lookup for speed
		}
	}
	
	static public string Get( string id )
	{
		int tgtHash = Animator.StringToHash( id );

		// TODO: binary chop this instead of linear iteration - CP
		for (int i=0; i<locTriplets.Count; i++)
		{
			if (locTriplets[i].Matches(tgtHash))
				if (locTriplets[i].Matches(id))
				{
		#if UNITY_EDITOR
					if (locTriplets[i].GetString().Length == 0)
						return "<" + locTriplets[i].GetId() + "> MISSING";
		#endif
					return locTriplets[i].GetString();
				}
		}

		#if UNITY_EDITOR
		return "<" + id + "> MISSING";
		#else
		return null;
		#endif
	}

	static public int GetSupportedCultures()
	{
		return cultures.Count;
	}

	static public string GetCultureCode( int n )
	{
		return cultures[n].GetId();
	}

	static public string GetCultureName( int n )
	{
		// culture string is actually an id for the localised culture name
		return Get( cultures[n].GetString() );
	}

}
