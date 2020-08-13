using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Noon;
using UnityEngine;


public class LanguageTable : MonoBehaviour
{
    //public static string csvFile = "content/strings.csv";
    public static string targetCulture = NoonConstants.DEFAULT_CULTURE;

    //private static List<LocTriplet> locTriplets;
    //private static List<LocTriplet> cultures;

    //public static string[] SplitCSVLine(string input)
    //{
    //	// The standard string.Split(',') does not correctly handle commas inside quotes,
    //	// so I've made a bespoke function (courtesy of StackOverflow for the regex because OMG NO) 
    //	Regex csvSplit = new Regex("(?:^|,)(\"(?:([^\"]|\"\")*)*\"|[^,]*)");
    //	List<string> list = new List<string>();
    //	string curr = null;
    //	foreach (Match match in csvSplit.Matches(input))
    //	{        
    //		curr = match.Value;
    //		if (0 == curr.Length)
    //		{
    //			list.Add("");
    //		}

    //		list.Add(curr.TrimStart(',').TrimStart('"').TrimEnd('"').TrimEnd('\r'));
    //	}

    //	return list.ToArray();
    //}

    public static void LoadCulture(string newTargetCulture)
    {
        targetCulture = newTargetCulture;

    }

    public static string Get( string id )
    {

        var currentCulture = Registry.Retrieve<ICompendium>().GetEntityById<Culture>(targetCulture);


        if (currentCulture.UILabels.TryGetValue(id.ToLower(), out string localisedValue))
            return localisedValue;

        if (currentCulture.Id != NoonConstants.DEFAULT_CULTURE )
        {
            var defaultCulture = Registry.Retrieve<ICompendium>().GetEntityById<Culture>(NoonConstants.DEFAULT_CULTURE);
            if (defaultCulture.UILabels.TryGetValue(id, out string defaultCultureValue))
                return defaultCultureValue;

        }

        
        return "MISSING_" + id.ToUpper();


        //if (locTriplets == null)
        //{
        //	Debug.LogWarning("locTriplets is null!? Is something holding strings.csv open?");
        //	//PlayerPrefs.SetString( "Culture", "en" );
        //	return "ERROR";
        //}

        //int tgtHash = Animator.StringToHash( id );

        //// TODO: binary chop this instead of linear iteration - CP
        //for (int i=0; i<locTriplets.Count; i++)
        //{
        //	if (locTriplets[i].Matches(tgtHash))
        //		if (locTriplets[i].Matches(id))
        //		{
        //#if UNITY_EDITOR
        //			if (locTriplets[i].GetString().Length == 0)
        //				return "<" + locTriplets[i].GetId() + "> MISSING";
        //#endif
        //			return locTriplets[i].GetString();
        //		}
        //}

        //#if UNITY_EDITOR
        //return "<" + id + "> MISSING";
        //#else
        //return null;
        //#endif
	}

	//static public int GetSupportedCulturesCount()
	//{
	//	return cultures.Count;
	//}

	//static public string GetCultureCode( int n )
	//{
	//	return cultures[n].GetId();
	//}

	//static public string GetCultureName( int n )
	//{
	//	// culture string is actually an id for the localised culture name
	//	return Get( cultures[n].GetString() );
	//}

}
