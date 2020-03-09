using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using UnityEngine;
using Assets.CS.TabletopUI;

namespace Noon
{
    public class NoonConstants
    {
        public const string KSTARTINGVERBID = "startingVerbId";
        public const string KCHARACTERSTATE = "state";
        public const string KLINKED="linked";
        public const string KENDING="ending";
        public const string KSIGNALENDINGFLAVOUR = "signalEndingFlavour";
        public const string KMAXEXECUTIONS = "maxexecutions";
        public const string KID = "id";
        public const string KFILTERONASPECTID = "filterOnAspectId";
        public const string KFILTERONASPECTIDALT = "filter";
        public const string KMUTATEASPECTID = "mutateAspectId";
        public const string KMUTATEASPECTIDALT = "mutate";
        public const string KMUTATIONLEVEL = "mutationLevel";
        public const string KMUTATIONLEVELALT = "level";
        public const string KADDITIVE = "additive";
        public const string KLABEL = "label";
        public const string KIMAGE = "image";
        public const string KFROMENDING = "fromEnding";
        public const string KAVAILABLEWITHOUTENDINGMATCH = "availableWithoutEndingMatch";
	    public const string KEXCLUDESONENDING = "excludesOnEnding";
        public const string KSTATUSBARELEMENTS = "statusbarelements";
        public const string KACTIONID = "actionId";
        public const string KCRAFTABLE = "craftable";
        public const string KPORTALEFFECT = "portaleffect";
        public const string KHINTONLY = "hintonly";
        public const string KSTARTDESCRIPTION = "startdescription";
        public const string KDESCRIPTION = "description";
        public const string KSIGNALIMPORTANTLOOP = "signalimportantloop";
        public const string KWARMUP = "warmup";
        public const string KREQUIREMENTS = "requirements";
        public const string KTABLEREQS = "tablereqs";
        public const string KEXTANTREQS = "extantreqs";
        public const string KEFFECTS = "effects";
        public const string KMUTATIONS = "mutations";
        public const string KDECKEFFECT = "deckeffect";
        public const string KPURGE = "purge";
        public const string KHALTVERB = "haltverb";
        public const string KDELETEVERB = "deleteverb";
        public const string KINTERNALDECK = "internaldeck";
        public const string KASPECTS = "aspects";
        public const string KXTRIGGERS = "xtriggers";
        public const string KVERBOVERRIDEICON = "verbicon";
        public const string KALTERNATIVERECIPES = "alternativerecipes";
        public const string KALTERNATIVERECIPESALT = "alt";
        public const string KSLOTS="slots";
        public const string KREQUIRED="required";
        public const string KFORBIDDEN = "forbidden";
        public const string KGREEDY = "greedy";
        public const string KCONSUMES = "consumes";
        public const string KNOANIM = "noanim";
        public const string KBURNIMAGE = "burnimage";
        public const string KDECKSPEC = "spec";
        public const string KDECKSPEC_DRAWMESSAGES = "drawmessages";
        public const string KDECKSPEC_DEFAULTDRAWMESSAGES = "defaultdrawmessages";
        public const string KDECKDEFAULTCARD = "defaultcard";
        public const string KDECKDEFAULTDRAWS = "draws";
        public static string KRESETONEXHAUSTION = "resetonexhaustion";
        public static string KICON="icon";
        public static string KEXPULSION="expulsion";
        public static string KLIMIT = "limit";
        public static string KFILTER = "filter";
        public static string KCHALLENGES="challenges";
        public static string DEFAULT_STARTING_VERB_ID="work";

        public const string KINDUCES = "induces";




        public const string KCHANCE = "chance";
        public const string KADDITIONAL = "additional";

        public const string KLIFETIME = "lifetime";
        public const string KDECAYTO = "decayTo";
        public const string KANIMFRAMES = "animFrames";
        public const string KISASPECT = "isAspect";
        public const string KISHIDDEN = "isHidden";
        public const string KNOARTNEEDED = "noartneeded";
        public const string KRESATURATE = "resaturate";

        public const string KUNIQUE = "unique";
        public const string KUNIQUENESSGROUP = "uniquenessgroup";


        public const string TOKEN_PREVIOUS_CHARACTER_NAME = "#PREVIOUSCHARACTERNAME#";
        public const string TOKEN_LAST_DESIRE = "#LAST_DESIRE#";
        public const string TOKEN_LAST_TOOL = "#LAST_TOOL#";
        public const string TOKEN_LAST_BOOK = "#LAST_BOOK#";
        public const string TOKEN_LAST_SIGNIFICANT_PAINTING = "#LAST_SIGNIFICANT_PAINTING#";
        public const string TOKEN_LAST_FOLLOWER= "#LAST_FOLLOWER#";
        public const string TOKEN_LAST_CULT = "#LAST_CULT#";
        public const string TOKEN_LAST_HEADQUARTERS = "#LAST_HEADQUARTERS#";
        public const string TOKEN_LAST_PERSON_KILLED_NAME = "#LAST_PERSON_KILLED_NAME#";

        public const string STOREFRONT_PATH_IN_STREAMINGASSETS = "edition/store.txt";

        public const string BIRDWORMSLIDER = "BirdWormSlider";
		public const string HIGHCONTRAST = "HighContrast";
		public const string ACCESSIBLECARDS = "AccessibleCards";


        public const string DECK_PREFIX = "deck:";
        public const string LEVER_PREFIX = "LEVER_";
        public const string MANSUS_DECKID_PREFIX = "mansus_";
        public const string SupportEmail = "support@weatherfactory.biz";

		public const string KANIM = "anim";
		public const string KFLAVOUR = "flavour";
		public const string KACHIEVEMENT = "achievement";


        public const string A_ENDING_MAJORFORGEVICTORY = "A_ENDING_MAJORFORGEVICTORY";
        public const string A_ENDING_MAJORGRAILVICTORY = "A_ENDING_MAJORGRAILVICTORY";
        public const string A_ENDING_MAJORLANTERNVICTORY = "A_ENDING_MAJORLANTERNVICTORY";

        public const string A_ENDING_MAJORVICTORYGENERIC = "A_MANSUS_TRICUSPIDGATE";
    }

    public enum VerbosityLevel
    {
        Essential=1,
        SystemChatter=8,
        Trivia=10
    }

    public class NoonUtility
    {
        public static bool UnitTestingMode { get; set; }

        public static int CurrentVerbosity =1;

        public static VersionNumber VersionNumber
        {
	        get { return new VersionNumber(Application.version); }
        }
        public static bool AchievementsActive = true;
        public static bool PerpetualEdition = false;

		// Centralised the time string creation so it's 100% consistent throughout game - CP
		private static bool stringsUpdated = false;
		private static string fixedspace = "<mspace=1.6em>";	// defaults are overriden by strings.csv
		private static string secondsPostfix = "s";
		private static string timeSeperator = ".";

		private static string[] screenLog;
		private static int screenLogStart = 0;
		private static bool screenLogVisible = false;

		static NoonUtility()
		{
			LanguageManager.LanguageChanged += OnLanguageChanged;
		}

        public static void Log(string message, int verbosityNeeded=0, int messageLevel=0)
        {
            if(verbosityNeeded<=CurrentVerbosity)
            {
				if (message != null)
				{
					// Store in on-screen log
					if (screenLog == null)
					{
						screenLog = new string[40];
					}
					screenLog[screenLogStart++] = message;
					if (screenLogStart >= screenLog.Count())
						screenLogStart = 0;
				}

	            //switch between in-Unity and unit testing
				if(UnitTestingMode)
					Console.WriteLine(message);
	            else
				{
					string formattedMessage =
						(verbosityNeeded > 0 ? new String('>', verbosityNeeded) + " " : "") + message;
		            switch (messageLevel)
		            {
			            case 0:
				            Debug.Log(formattedMessage);
				            break;
			            case 1:
				            Debug.LogWarning(formattedMessage);
				            break;
			            case 2:
				            Debug.LogError(formattedMessage);
				            break;
			            default:
				            throw new ArgumentOutOfRangeException("messageLevel");
		            }
	            }
            }
        }
        public static void Log(string message, VerbosityLevel verbosityNeeded, int messageLevel=0)
        {
            Log(message,Convert.ToInt32(verbosityNeeded), messageLevel);
        }

        public static string GetGameSaveLocation(int index = 0)
        {
            string wholePath = Application.persistentDataPath + "/save" + (index == 0 ? "": "_" + index) + ".txt" ;
            return wholePath;
        }

        public static string GetTemporaryGameSaveLocation(int index = 0)
        {
	        string wholePath = Application.persistentDataPath + "/save_tmp" + (index == 0 ? "": "_" + index) + ".txt" ;
	        return wholePath;
        }

        public static string GetBackupGameSaveLocation( int index )
        {
		    string wholePath = Application.persistentDataPath + "/backup_save" + index + ".txt";
            return wholePath;
        }

        public static string GetErrorSaveLocation( DateTime timestamp, string postfix )
        {
		    string wholePath = Application.persistentDataPath + "/error_save_" + timestamp.ToString("yyyyMMdd_HHmmss") + "_" + postfix + ".txt";
            return wholePath;
        }

        public static Dictionary<string, int> HashtableToStringIntDictionary(Hashtable table)
        {
            if (table == null)
                return null;

            var dictionary=table
                .Cast<DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString(), kvp => int.Parse(kvp.Value.ToString()));

            return dictionary;

        }

        public static string JoinPaths(params string[] paths)
        {
            return paths.Aggregate("", Path.Combine);
        }

        public static Dictionary<string, string> HashtableToStringStringDictionary(Hashtable table)
        {
            if (table == null)
                return null;

            var dictionary = table
                .Cast<DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString());

            return dictionary;

        }

        public static void CopyDirectoryRecursively(string source, string destination, bool move = false)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(source);
            DirectoryInfo destinationDirectory = new DirectoryInfo(destination);
            if (!destinationDirectory.Exists)
                destinationDirectory.Create();
            foreach (var file in sourceDirectory.GetFiles().Where(IsPermittedFileToCopy))
            {
                if (move)
                    file.MoveTo(NoonUtility.JoinPaths(destination, file.Name));
                else
                    file.CopyTo(NoonUtility.JoinPaths(destination, file.Name), true);
            }

            foreach (var directory in sourceDirectory.GetDirectories()
                .Where(d => !destinationDirectory.FullName.StartsWith(d.FullName)))
            {
                CopyDirectoryRecursively(directory.FullName, NoonUtility.JoinPaths(destination, directory.Name), move);
                if (move)
                    Directory.Delete(directory.FullName);
            }
        }

        private static bool IsPermittedFileToCopy(FileSystemInfo file)
        {
            return file.Name != ".dropbox";
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

		public static Vector3 SnapToGrid( Vector3 v )
		{
			if (TabletopManager.GetGridSnapSize() > 0f)
			{
				// Magical maths to snap cards to fractions of approx card dimensions - CP
				float snap_x = 90.0f * TabletopManager.GetGridSnapSize();
				float snap_y = 130.0f * TabletopManager.GetGridSnapSize();
				float recip_x = 1.0f / snap_x;
				float recip_y = 1.0f / snap_y;
				v.x *= recip_x;	v.x = (float)Mathf.RoundToInt(v.x);	v.x *= snap_x;
				v.y *= recip_y;	v.y = (float)Mathf.RoundToInt(v.y);	v.y *= snap_y;
			}
			return v;
		}

		public static string MakeTimeString( float time )
		{
			if (!stringsUpdated)	// Slightly clumsy one-time lookup of strings, but this way they are guaranteed to be localised on first use and never looked up again
			{
				// One-time lookup of static strings, on first time request only
				fixedspace		= LanguageTable.Get("UI_FIXEDSPACE");				// Contains rich text fixed spacing size (and <b> for some langs)
				secondsPostfix	= LanguageTable.Get("UI_SECONDS_POSTFIX_SHORT");	// Contains localised abbreviation for seconds, maybe a space and maybe a </b>
				timeSeperator	= LanguageTable.Get("UI_TIME_SEPERATOR");			// '.' for most langs but some prefer ','
				stringsUpdated	= true;
			}

			string s = time.ToString("0.0");
			s = s.Replace( '.', timeSeperator[0] );
			return fixedspace + s + secondsPostfix;
		}

		public static void ToggleLog()
		{
			screenLogVisible = !screenLogVisible;
		}

		public static void DrawLog()
		{
			if (!screenLogVisible)
				return;

			for (int i=0; i<screenLog.Count(); i++)
			{
				int idx = (screenLogStart+1+i) % screenLog.Count();
				GUI.Label( new Rect(0, Screen.height - 60f - (20f * screenLog.Count()) + (20f*i), Screen.width, 20), screenLog[idx]);
			}
		}

		private static void OnLanguageChanged()
		{
			stringsUpdated = false;
		}
    }

}

