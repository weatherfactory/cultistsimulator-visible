using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

public class NoonUtilityTests {
    private const string ASPECT_ID = "moth";
    private const int QUANTITY_TINY = 1;
    private const int QUANTITY_SMALL = 2;
    private const int QUANTITY_MODEST = 3;
    private const int QUANTITY_GENEROUS = 4;
    private const int QUANTITY_SIGNIFICANT = 6;
    private const int QUANTITY_MAJOR = 8;
    private const int QUANTITY_EPISODE_END = 7;


    [Test]
    public void UtilityDiscernsConventionValues()
    {
        Hashtable htAspects = new Hashtable()
        {
        { ASPECT_ID + QUANTITY_TINY, "CONVENTIONS.QUANTITY_TINY" },
            { ASPECT_ID + QUANTITY_SMALL, "CONVENTIONS.QUANTITY_TINY" },
            { ASPECT_ID + QUANTITY_MODEST, "CONVENTIONS.QUANTITY_MODEST" },
            { ASPECT_ID + QUANTITY_GENEROUS, "CONVENTIONS.QUANTITY_GENEROUS" },
            { ASPECT_ID + QUANTITY_SIGNIFICANT, "CONVENTIONS.QUANTITY_SIGNIFICANT" },
            { ASPECT_ID + QUANTITY_MAJOR, "CONVENTIONS.QUANTITY_MAJOR" },
            { ASPECT_ID + QUANTITY_EPISODE_END, "CONVENTIONS.QUANTITY_EPISODE_END" },

            };
        Dictionary<string,int> Results = Noon.Utility.ReplaceConventionValues(htAspects);
        Assert.AreEqual(Results[ASPECT_ID + QUANTITY_TINY], QUANTITY_TINY);
        Assert.AreEqual(Results[ASPECT_ID + QUANTITY_MODEST], QUANTITY_MODEST);
        Assert.AreEqual(Results[ASPECT_ID + QUANTITY_GENEROUS], QUANTITY_GENEROUS);
        Assert.AreEqual(Results[ASPECT_ID + QUANTITY_SIGNIFICANT], QUANTITY_SIGNIFICANT);
        Assert.AreEqual(Results[ASPECT_ID + QUANTITY_MAJOR], QUANTITY_MAJOR);
        Assert.AreEqual(Results[ASPECT_ID + QUANTITY_EPISODE_END], QUANTITY_EPISODE_END);


    }

    [Test]
    public void UtilityConvertsFromHashTableToDictionary()
    {

        Hashtable htAspects = new Hashtable()
        {
        { ASPECT_ID, QUANTITY_TINY.ToString() }
            };
        Dictionary<string, int> result = Noon.Utility.JSONHashtableToIntValuesDictionary(htAspects);

        Assert.AreEqual(result[ASPECT_ID], QUANTITY_TINY);

    }



}
