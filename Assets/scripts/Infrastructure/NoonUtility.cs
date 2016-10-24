using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Noon
{

    public class NoonUtilityConstants
    {
        public static string CONST_SAVE_RECIPESKNOWN="recipesKnown";
        public const string CONST_SAVE_ELEMENTSPOSSESSED = "elementsPossessed";
        public const string CONST_SAVE_RECIPETIMERS = "recipeTimers";
    }
    public class NoonUtility
    {
        public static string GetGameSavePath()
        {
            return Application.persistentDataPath + "\\savedgame.txt";
        }


        public static Dictionary<string, int> JSONHashtableToIntValuesDictionary(Hashtable table)
        {
            return table
                .Cast<DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString(), kvp => Convert.ToInt32(kvp.Value));
        }

        public static Dictionary<string,int> ReplaceConventionValues(Hashtable htAspects)
        {

            Dictionary<string,int> Results=new Dictionary<string,int>();
            if (htAspects == null)
                return Results;
            foreach (string k in htAspects.Keys)
            {
                string v = htAspects[k].ToString();
                int intV;
                switch (v)
                {
                    case "CONVENTIONS.QUANTITY_TINY":
                        intV = 1;
                        break;
                    case "CONVENTIONS.QUANTITY_SMALL":
                        intV = 2;
                        break;
                    case "CONVENTIONS.QUANTITY_MODEST":
                        intV = 3;
                        break;
                    case "CONVENTIONS.QUANTITY_GENEROUS":
                        intV = 4;
                        break;
                    case "CONVENTIONS.QUANTITY_SIGNIFICANT":
                        intV = 6;
                        break;
                    case "CONVENTIONS.QUANTITY_MAJOR":
                        intV = 8;
                        break;
                    case "CONVENTIONS.QUANTITY_EPISODE_END":
                        intV = 7;
                        break;
                    default:
                        intV = Convert.ToInt32(v);
                        break;
                }

                Results.Add(k,intV);
            }
            return Results;


        }
    }

}

