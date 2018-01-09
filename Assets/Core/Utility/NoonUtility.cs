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
        public const string KCHARACTERSTATE = "state";
        public const string KLINKED="linked";
        public const string KENDING="ending";
        public const string KMAXEXECUTIONS = "maxexecutions";
        public const string KID = "id";
        public const string KLABEL = "label";
        public const string KIMAGE = "image";
        public const string KACTIONID = "actionId";
        public const string KCRAFTABLE = "craftable";
        public const string KSTARTDESCRIPTION = "startdescription";
        public const string KDESCRIPTION = "description";
        public const string KASIDE = "aside";
        public const string KWARMUP = "warmup";
        public const string KREQUIREMENTS = "requirements";
        public const string KEFFECTS = "effects";
        public const string KDECKEFFECT = "deckeffect";
        public const string KASPECTS = "aspects";
        public const string KXTRIGGERS = "xtriggers";
        public const string KALTERNATIVERECIPES = "alternativerecipes";
        public const string KPERSISTINGREDIENTSWITH = "persistIngredientsWith";
        public const string KRETRIEVESCONTENTSWITH = "retrievesContentsWith";
        public const string KSLOTS="slots";
        public const string KREQUIRED="required";
        public const string KFORBIDDEN = "forbidden";
        public const string KGREEDY = "greedy";
        public const string KCONSUMES = "consumes";
        public const string KBURNIMAGE = "burnimage";
        public const string KDECKSPEC = "spec";
        public const string KDECKDEFAULTCARD = "defaultcard";
        public static string KRESETONEXHAUSTION = "resetonexhaustion";
        public const string KINDUCES = "induces";


        public const string KCHANCE = "chance";
        public const string KADDITIONAL = "additional";
        public const string KCHARACTERTITLE = "title";
        public const string KCHARACTERFIRSTNAME= "firstname";
        public const string KCHARACTERLASTNAME = "lastname";

        public const string TOKEN_PREVIOUS_CHARACTER_NAME = "#PREVIOUSCHARACTERNAME#";

        public const string DECK_PREFIX = "deck:";
    }
    public class NoonUtility
    {
        public static bool UnitTestingMode { get; set; }
        public static int CurrentVerbosity = 5;

        public static string VersionNumber = "2017.12.b.3";

        public static void Log(string message,int verbosityNeeded=0)
        {
            if(verbosityNeeded<CurrentVerbosity)
            { 
            //switch between in-Unity and unit testing
            if(UnitTestingMode)
            Console.WriteLine(message);
            else
            Debug.Log(">>>>> " + message);
            }
        }

        public static string GetGameSaveLocation()
        {
            string wholePath= Application.persistentDataPath + "/save.txt" ;
            return wholePath;
        }

        public static string GetBackupGameSaveLocation()
        {
            string wholePath = Application.persistentDataPath + "/backup_save.txt";
            return wholePath;
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

