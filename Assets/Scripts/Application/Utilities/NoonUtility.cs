using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using  SecretHistories.Fucine;



    public class NoonLogMessage : ILogMessage
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


 

    public enum VerbosityLevel
    {
        Essential=1,
        Significants=4,
        SystemChatter=8,
        Trivia=10
    }

    public interface ILogSubscriber
    {
        /// <summary>
        /// Ignore messages of verbosity>sensitivity
        /// </summary>
        VerbosityLevel Sensitivity { get; }
        void AddMessage(ILogMessage message);
    }

    public class NoonUtility
    {

        public static bool AchievementsActive = true;
        public static bool PerpetualEdition = false;


        private static List<ILogSubscriber> subscribers =new List<ILogSubscriber>();


        public static void Subscribe(ILogSubscriber subscriber)
        {
            subscribers.Add(subscriber);
        }
        
        public static void Log(ILogMessage message)
        {
            if (message == null)
                message=new NoonLogMessage("Null log message supplied");

      
                foreach(var s in subscribers)
                {
                    if((int)message.VerbosityNeeded<=(int)s.Sensitivity)
                        s.AddMessage(message);
                

        }

        }


        public static void Log(string description, int messageLevel=0, VerbosityLevel verbosityNeeded=VerbosityLevel.Trivia)
        {
            ILogMessage message = new NoonLogMessage(description, messageLevel, Convert.ToInt32(verbosityNeeded));

            Log(message);
        }

        public static void LogWarning(string description)
        {
            ILogMessage message = new NoonLogMessage(description, 1, Convert.ToInt32(VerbosityLevel.Essential));

            Log(message);
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
            ILogMessage logMessage=new NoonLogMessage(description ,2,0);
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


