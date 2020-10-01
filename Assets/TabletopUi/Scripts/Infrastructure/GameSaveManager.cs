using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using OrbCreationExtensions;
using UnityEngine;	// added for debug asserts - CP
using UnityEngine.Analytics;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface ITableSaveState
    {
        IEnumerable<ElementStackToken> TableStacks { get; }
        List<SituationController> Situations { get; }
        bool IsTableActive();
    }

    public class TableSaveState : ITableSaveState
    {
        public IEnumerable<ElementStackToken> TableStacks { get; private set; }
        public List<SituationController> Situations { get; private set; }
        public bool IsTableActive()
        {
            return true;
        }


        public TableSaveState(IEnumerable<ElementStackToken> tableStacks, List<SituationController> situations)
        {
            TableStacks = tableStacks;
            Situations = situations;
        }
    }

    public class InactiveTableSaveState : ITableSaveState
    {
        public IEnumerable<ElementStackToken> TableStacks { get; private set; }
        public List<SituationController> Situations { get; private set; }

        public bool IsTableActive()
        {
            return false;
        }

        public InactiveTableSaveState()
        {
            TableStacks = new List<ElementStackToken>();
            Situations = new List<SituationController>();
        }


    }


    public class GameSaveManager
    {
        private readonly IGameDataImporter dataImporter;
        private readonly IGameDataExporter dataExporter;

		// Save game safety
		public static int failedSaveCount = 0;
		public static bool saveErrorWarningTriggered = false;	// so that tabletop knows not to unpause
#if UNITY_EDITOR
		public static bool simulateBrokenSave = false;	// For debugging
#endif
        public GameSaveManager(IGameDataImporter dataImporter,IGameDataExporter dataExporter)
        {
            this.dataImporter = dataImporter;
            this.dataExporter = dataExporter;
        }

        public bool DoesGameSaveExist()
        {
            return File.Exists(NoonUtility.GetGameSaveLocation());
        }

        public bool IsSavedGameActive(SourceForGameState source, bool temp = false)
        {
            var htSave = RetrieveHashedSaveFromFile(source, temp);
            return htSave.ContainsKey(SaveConstants.SAVE_ELEMENTSTACKS) || htSave.ContainsKey(SaveConstants.SAVE_SITUATIONS);
        }

        //copies old version in case of corruption
        private void BackupSave(int index = 0)
        {
			const int MAX_BACKUPS = 5;
			// Back up a number of previous saves
            for (int i=MAX_BACKUPS-1; i>=1; i--)
			{
				if (File.Exists(NoonUtility.GetBackupGameSaveLocation(i)))	//otherwise we can't copy it
	                File.Copy  (NoonUtility.GetBackupGameSaveLocation(i), NoonUtility.GetBackupGameSaveLocation(i+1),true);
			}
			// Back up the main save
			if (File.Exists(NoonUtility.GetGameSaveLocation(index)))	//otherwise we can't copy it
                File.Copy  (NoonUtility.GetGameSaveLocation(index), NoonUtility.GetBackupGameSaveLocation(1),true);
		}



        public void DeleteCurrentSave()
        {
            if(File.Exists(NoonUtility.GetGameSaveLocation()))
                File.Delete(NoonUtility.GetGameSaveLocation());
        }

        //  public IEnumerator<bool?> SaveActiveGameAsync(ITableSaveState tableSaveState, Character character, bool forceBadSave = false, int index = 0)
        public async Task<bool> SaveActiveGameAsync(ITableSaveState tableSaveState, Character character, SourceForGameState source, bool forceBadSave = false)
        {

            int index = (int) source;
              //  var allStacks = tabletop.GetElementStacksManager().GetStacks();
              //  var currentSituationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();
                var metaInfo = Registry.Get<MetaInfo>();
      
                // GetSaveHashTable now does basic validation of the data and might return null if it's bad - CP
                var htSaveTable = dataExporter.GetSaveHashTable(metaInfo,tableSaveState, character);

                // Universal catch-all. If data is broken, abort save and retry 5 seconds later - assuming problem circumstance has completed - CP
                // If we fail several of these in a row then something is permanently broken in the data and we should alert user :(
                if (htSaveTable == null && !forceBadSave)
                {
                    HandleSaveError(tableSaveState, character, index);
                    return false;	// Something went wrong with the save
                    
                }
                if (forceBadSave)
                {
                    var postSaveFilePath = NoonUtility.GetErrorSaveLocation(DateTime.Now, "post");
                    var writeFileTask=WriteSaveFile(postSaveFilePath, htSaveTable);
                    await writeFileTask;

                }
                else
                {
                    // Write the save to a temporary file first, check that it is valid, and only then backup the old save
                    // This is to mitigate some bugs where save files would end up with invalid data written to them
                    var saveFilePath = NoonUtility.GetGameSaveLocation(index);
                    var tempSaveFilePath = NoonUtility.GetTemporaryGameSaveLocation(index);
                    var saveTask = WriteSaveFile(tempSaveFilePath, htSaveTable);

                    await saveTask;

                    //while (!(saveTask.IsCompleted || saveTask.IsCompleted || saveTask.IsFaulted))
                    //{
                    //    return false;
                    //}

                    bool success;
                    if (saveTask.Exception == null)
                    {
                        success = IsSavedGameActive(source, true);
                    }
                    else
                    {
                        Debug.LogError("Failed to save game to temporary save file (see exception for details)");
                        Debug.LogException(saveTask.Exception);
                        success = false;
                    }

                    if (success)
                    {
                        try
                        {
                            BackupSave(index);
                            if (File.Exists(saveFilePath))
                            {
                                File.Delete(saveFilePath);
                            }

                            File.Move(tempSaveFilePath, saveFilePath);

                            if (failedSaveCount > 0)
                            {
                                Analytics.CustomEvent( "autosave_recovered", new Dictionary<string,object>{ { "failedSaveCount", failedSaveCount } } );
                            }

                            failedSaveCount = 0;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Failed to move temporary file (see exception for details)");
                            Debug.LogException(e);
                            success = false;
                        }
                    }
                    if (!success)
                    {
                        HandleSaveError(tableSaveState, character, index);
                        return false;
                    }
                }

                return true;
        }

        public void LoadTabletopState(TabletopTokenContainer tabletop,SourceForGameState source)
        {
            var htSave = RetrieveHashedSaveFromFile(source);
            dataImporter.ImportTableState(tabletop,htSave);
        }

        public Character LoadCharacterState(SourceForGameState source)
        {
            var htSave = RetrieveHashedSaveFromFile(source);
           return dataImporter.ImportCharacter(htSave);
        }



        private Hashtable RetrieveHashedSaveFromFile(SourceForGameState source, bool temp = false)
        {
            var index = (int) source;

            string importJson = File.ReadAllText(
	            temp ? NoonUtility.GetTemporaryGameSaveLocation(index) : NoonUtility.GetGameSaveLocation(index));
            Hashtable htSave = SimpleJsonImporter.Import(importJson);
            return htSave;
        }





        private async void HandleSaveError(ITableSaveState tableSaveState, Character character, int index = 0)
        {
	        failedSaveCount++;
	        if (failedSaveCount != 3) 
		        return;
	        
	        // Back up main save
            var savePath = NoonUtility.GetGameSaveLocation(index);
            if (File.Exists(savePath))  // otherwise we can't copy it
	            File.Copy(savePath, NoonUtility.GetErrorSaveLocation(DateTime.Now, "pre"), true);
                
            // Force a bad save into a different filename
            var saveTask = SaveActiveGameAsync(tableSaveState, character, SourceForGameState.DefaultSave,true);

            var success = await saveTask;

       if(!success)
       {
            Analytics.CustomEvent("autosave_corrupt_notified");
                
            // Pop up warning message
            Registry.Get<INotifier>().ShowSaveError(true);
            saveErrorWarningTriggered = true;
       }
        }

        private async Task WriteSaveFile(string saveFilePath, Hashtable saveData)
        {
	       var task=Task.Run(()=> File.WriteAllText(saveFilePath, saveData.JsonString()));
           await task;
        }


    }
}
