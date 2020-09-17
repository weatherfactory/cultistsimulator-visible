using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Noon
{
    public class NoonLogMessage
    {
        private string _description;

        public string Description
        {
            get { return _description; }
            set { _description = value ?? "Unspecified"; }
        }

        public int MessageLevel { get; private set; }
        public int VerbosityNeeded { get; private set; }
        public Exception LoggedException { get; set; }

        public NoonLogMessage(string description)
        {
            Description = description;
            MessageLevel = 2;
            VerbosityNeeded = (int) VerbosityLevel.SystemChatter;
        }

        public NoonLogMessage(string description, int messageLevel)
        {
            Description = description;
            MessageLevel = messageLevel;
            VerbosityNeeded = (int) VerbosityLevel.SystemChatter;
        }

        public NoonLogMessage(string description, int messageLevel,int verbosityNeeded)
        {
            Description = description;
            MessageLevel = messageLevel;
            VerbosityNeeded = verbosityNeeded;
        }

        public override string ToString()
        {
            return Description;
        }
    }


    public class NoonConstants
    {

        public const string TEMPLATE_MARKER = "$";
        
        public const string ID = "id";
        public const string UID = "uid";
        public const string EXTENDS = "extends";

        public const int CULTIST_STEAMWORKS_APP_ID = 718670;

        public const string CORE_FOLDER_NAME = "core";
        public const string LOC_FOLDER_TEMPLATE = "loc_[culture]";
        public const string LOC_TOKEN = "[culture]";
        
        public const string CONTENT_FOLDER_NAME = "content";
        public static string LOC_FOLDER_NAME="loc";
        public const string DEFAULT_CULTURE_ID = "en";
        public const string CULTURE_SETTING_KEY = "Culture";

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
        public const string CONSCIENCE_PATH_IN_STREAMINGASSETS = "edition/please_buy_our_game.txt";

        public const string WORKSHOP_ITEM_PUBLISHED_ID_FILE_NAME = "serapeum_catalogue_number.txt";
        public static string WORKSHOP_PREVIEW_IMAGE_FILE_NAME = "cover.png";

        public const string BIRDWORMSLIDER = "AllowExploits";
		public const string HIGHCONTRAST = "HighContrast";
		public const string ACCESSIBLECARDS = "AccessibleCards";
        public const string RESOLUTION = "Resolution";
        public const string WINDOWED = "Windowed";
        public const string GRAPHICSLEVEL = "GraphicsLevel";


        public const string DECK_PREFIX = "deck:";
        public const string LEVER_PREFIX = "lever_";
        public const string MANSUS_DECKID_PREFIX = "mansus_";
        public const string SupportEmail = "support@weatherfactory.biz";



        public const string A_ENDING_MAJORFORGEVICTORY = "A_ENDING_MAJORFORGEVICTORY";
        public const string A_ENDING_MAJORGRAILVICTORY = "A_ENDING_MAJORGRAILVICTORY";
        public const string A_ENDING_MAJORLANTERNVICTORY = "A_ENDING_MAJORLANTERNVICTORY";

        public const string A_ENDING_MAJORVICTORYGENERIC = "A_MANSUS_TRICUSPIDGATE";
        public const string MUSICVOLUME="MusicVolume";
        public const string SOUNDVOLUME = "SoundVolume";
        public const string NOTIFICATIONTIME = "NotificationTime";
        public const string SCREENCANVASSIZE = "ScreenCanvasSize";
        public const string AUTOSAVEINTERVAL = "AutosaveInterval";
        public const string GRIDSNAPSIZE = "GridSnapSize";
    }

    public enum VerbosityLevel
    {
        Essential=1,
        Significants=4,
        SystemChatter=8,
        Trivia=10
    }

    public interface ILogSubscriber
    {
        void AddMessage(NoonLogMessage message);
    }

    public class NoonUtility
    {
        public static bool UnitTestingMode { get; set; }

        public static int CurrentVerbosity =Convert.ToInt32(VerbosityLevel.Trivia);

        public static bool AchievementsActive = true;
        public static bool PerpetualEdition = false;


        private static List<ILogSubscriber> subscribers =new List<ILogSubscriber>();


        public static void Subscribe(ILogSubscriber subscriber)
        {
            subscribers.Add(subscriber);
        }
        
        public static void Log(NoonLogMessage message)
        {
            if (message == null)
                message=new NoonLogMessage("Null log message supplied");

            if(message.VerbosityNeeded <= CurrentVerbosity || CurrentVerbosity==0) //very early in the process - like, at content load - this doesn't seem to be set, because something something static classes
            {
                foreach(var s in subscribers)
                        s.AddMessage(message);

            }

            //switch between in-Unity and unit testing
            if(UnitTestingMode)
                Console.WriteLine(message.Description);
            else
            {
                string formattedMessage =
                    (message.VerbosityNeeded > 0 ? new String('>', message.VerbosityNeeded) + " " : "") + message.Description;
                switch (message.MessageLevel)
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
                        Debug.LogError(formattedMessage);
                        break;
                    //  throw new ArgumentOutOfRangeException("messageLevel " + messageLevel);
                }
            }
            
        }


        public static void Log(string description, int messageLevel=0, VerbosityLevel verbosityNeeded=VerbosityLevel.Trivia)
        {
            NoonLogMessage message = new NoonLogMessage(description, messageLevel, Convert.ToInt32(verbosityNeeded));

            Log(message);
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

        public static void LogException(Exception exception)
        {
            string description = $"{exception.Message}: \n {exception.StackTrace}";
            NoonLogMessage logMessage=new NoonLogMessage(description ,2,0);
            logMessage.LoggedException = exception;
            Log(logMessage);
        }
    }

    public static class AspectColor
    {
        public static Color32 Edge()
        {
            return new Color32(215, 221, 73, 255);
        }
        public static Color32 Forge()
        {
            return new Color32(255, 142, 62, 255);
        }
        public static Color32 Grail()
        {
            return new Color32(254, 97, 80, 255);
        }
        public static Color32 Heart()
        {
            return new Color32(254, 126, 139, 255);
        }
        public static Color32 Knock()
        {
            return new Color32(181, 78, 252, 255);
        }
        public static Color32 Lantern()
        {
            return new Color32(255, 227, 0, 255);
        }
        public static Color32 Moth()
        {
            return new Color32(242, 233, 194, 255);
        }
        public static Color32 SecretHistories()
        {
            return new Color32(255, 0, 144, 255);
        }

        public static Color32 Winter()
        {
            return new Color32(190, 238, 255, 255);
        }
    }
}

