using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using UnityEngine;

namespace Noon
{

    public class NoonConstants
    {
        public static string SAVE_ELEMENTID = "elementId";
        public static string SAVE_QUANTITY = "quantity";
        public static string SAVE_VERBID = "verbId";
        public static string SAVE_RECIPEID = "recipeId";
        public static string SAVE_SITUATIONSTATE="state";
        public static string SAVE_TIMEREMAINING = "timeremaining";


        public static string SAVE_RECIPESKNOWN="recipesKnown";
        public const string SAVE_ELEMENTSTACKS = "elementStacks";
        public const string SAVE_SITUATIONS = "situations";
        public static string SAVE_CHARACTER_DETAILS = "characterDetails";
        public static string KCHARACTERSTATE = "state";
        public static string KLOOP="loop";
        public static string KENDING="ending";
        public const string KID = "id";
        public const string KLABEL = "label";
        public const string KACTIONID = "actionId";
        public const string KCRAFTABLE = "craftable";
        public const string KSTARTDESCRIPTION = "startdescription";
        public const string KDESCRIPTION = "description";
        public const string KWARMUP = "warmup";
        public const string KREQUIREMENTS = "requirements";
        public const string KEFFECTS = "effects";
        public const string KALTERNATIVERECIPES = "alternativerecipes";
        public const string KPERSISTINGREDIENTSWITH = "persistIngredientsWith";
        public static string KRETRIEVESCONTENTSWITH = "retrievesContentsWith";
        public static string KSLOTS="slots";
        public static string KREQUIRED="required";
        public static string KFORBIDDEN = "forbidden";
        public static string KGREEDY = "greedy";
        
        public const string KCHANCE = "chance";
        public const string KADDITIONAL = "additional";
        public const string KCHARACTERTITLE = "title";
        public const string KCHARACTERFIRSTNAME= "firstname";
        public const string KCHARACTERLASTNAME = "lastname";



    }
    public class NoonUtility
    {


        public static string GetGameSavePath(string filename)
        {
            return Application.persistentDataPath + "\\" + filename;
        }



        public static Dictionary<string, int> HashtableToStringIntDictionary(Hashtable table)
        {
            var dictionary=table
                .Cast<DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString(), kvp => int.Parse(kvp.Value.ToString()));

            return dictionary;

        }

        public static Dictionary<string, string> HashtableToStringStringDictionary(Hashtable table)
        {
            var dictionary = table
                .Cast<DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString());

            return dictionary;

        }


        public static IAspectsDictionary ReplaceConventionValues(Hashtable htAspects)
        {

            IAspectsDictionary Results=new AspectsDictionary();
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

