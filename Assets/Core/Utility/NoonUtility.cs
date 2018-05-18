using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using UnityEngine;

namespace Noon
{

    public class NoonConstants
    {
        public const string KCHARACTERSTATE = "state";
        public const string KLINKED="linked";
        public const string KENDING="ending";
        public const string KSIGNALENDINGFLAVOUR = "signalEndingFlavour";
        public const string KMAXEXECUTIONS = "maxexecutions";
        public const string KID = "id";
        public const string KFILTERONASPECTID = "filterOnAspectId";
        public const string KMUTATEASPECTID = "mutateAspectId";
        public const string KMUTATIONLEVEL = "mutationLevel";
        public const string KADDITIVE = "additive";
        public const string KLABEL = "label";
        public const string KIMAGE = "image";
        public const string KFROMENDING = "fromEnding";
        public const string KAVAILABLEWITHOUTENDINGMATCH = "availableWithoutEndingMatch";
        public const string KACTIONID = "actionId";
        public const string KCRAFTABLE = "craftable";
        public const string KPORTALEFFECT = "portaleffect";
        public const string KHINTONLY = "hintonly";
        public const string KSTARTDESCRIPTION = "startdescription";
        public const string KDESCRIPTION = "description";
        public const string KSIGNALIMPORTANTLOOP = "signalimportantloop";
        public const string KWARMUP = "warmup";
        public const string KREQUIREMENTS = "requirements";
        public const string KEFFECTS = "effects";
        public const string KMUTATIONS = "mutations";
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
        public const string KDECKSPEC_DRAWMESSAGES = "drawmessages";
        public const string KDECKSPEC_DEFAULTDRAWMESSAGES = "defaultdrawmessages";
        public const string KDECKDEFAULTCARD = "defaultcard";
        public static string KRESETONEXHAUSTION = "resetonexhaustion";
        public static string KICON="icon";

        public const string KINDUCES = "induces";

        


        public const string KCHANCE = "chance";
        public const string KADDITIONAL = "additional";

        public const string KLIFETIME = "lifetime";
        public const string KDECAYTO = "decayTo";
        public const string KANIMFRAMES = "animFrames";
        public const string KISASPECT = "isAspect";
        public const string KNOARTNEEDED = "noartneeded";
        public const string KUNIQUE = "unique";


        public const string TOKEN_PREVIOUS_CHARACTER_NAME = "#PREVIOUSCHARACTERNAME#";
        public const string TOKEN_LAST_DESIRE = "#LAST_DESIRE#";
        public const string TOKEN_LAST_TOOL = "#LAST_TOOL#";
        public const string TOKEN_LAST_BOOK = "#LAST_BOOK#";
        public const string TOKEN_LAST_SIGNIFICANT_PAINTING = "#LAST_SIGNIFICANT_PAINTING#";
        public const string TOKEN_LAST_FOLLOWER= "#LAST_FOLLOWER#";
        public const string TOKEN_LAST_CULT = "#LAST_CULT#";
        public const string TOKEN_LAST_HEADQUARTERS = "#LAST_HEADQUARTERS#";
        public const string TOKEN_LAST_PERSON_KILLED_NAME = "#LAST_PERSON_KILLED_NAME#";

        public const string BIRDWORMSLIDER = "BirdWormSlider";


        public const string DECK_PREFIX = "deck:";
        public const string LEVER_PREFIX = "LEVER_";
        public const string MANSUS_DECKID_PREFIX = "mansus_";
        public const string SupportEmail = "support@weatherfactory.biz";
    }
    public class NoonUtility
    {
        public static bool UnitTestingMode { get; set; }
        

        public static int CurrentVerbosity =1;
        //public static uint CultistSimulatorSteamAppId = 718670;
        //public static string CultistSimulatorGOGProductId = "1456702644";
        //public static string CultistSimulatorGOGClientId = "50757209545787544";
        //public static string CultistSimulatorGOGClientSecret = "72e691b01ad6060c8716bb4155b305c68048585aae07d1227eecc5a6c959161c";

        public static VersionNumber VersionNumber = new VersionNumber(Application.version);
        public static bool AchievementsActive = false;

        public static void Log(string message,int verbosityNeeded=0)
        {
            if(verbosityNeeded<=CurrentVerbosity)
            { 
            //switch between in-Unity and unit testing
            if(UnitTestingMode)
            Console.WriteLine(message);
            else
            Debug.Log(new String('>',verbosityNeeded) + " " + message);
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

        public static void WormWar(float value)
        {
            //hi.
        }
    }

}

