using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using OrbCreationExtensions;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class GameSaveManager
    {
        private readonly IGameDataImporter dataImporter;
        private readonly IGameDataExporter dataExporter;

        
        public GameSaveManager(IGameDataImporter dataImporter,IGameDataExporter dataExporter)
        {
            this.dataImporter = dataImporter;
            this.dataExporter = dataExporter;
        }

        public bool DoesGameSaveExist()
        {
            return File.Exists(NoonUtility.GetGameSaveLocation());
        }

        public bool IsSavedGameActive()
        {
            var htSave = RetrieveHashedSaveFromFile();
            return htSave.ContainsKey(SaveConstants.SAVE_ELEMENTSTACKS) || htSave.ContainsKey(SaveConstants.SAVE_SITUATIONS);
        }

        //copies old version in case of corruption
        private void BackupSave()
        {
            if(DoesGameSaveExist()) //otherwise we can't copy it
                File.Copy(NoonUtility.GetGameSaveLocation(), NoonUtility.GetBackupGameSaveLocation(),true);
       }
        
        /// <summary>
        /// for saving from the game over or legacy choice screen, when the player is between active games. It's also used when restarting the game
        /// by reloading the tabletop scene - this is hacky, because we just don't save the availablelegacies and chosenlegacies, and rely on the
        /// ChosenLegacy remaining in cross-scene state. It would be better to go via  somewhere that inspects state and routes accordingly
        /// </summary>
        public void SaveInactiveGame()
        {
            BackupSave();
            
            try
            {
                var htSaveTable = dataExporter.GetHashtableForExtragameState();
                File.WriteAllText(NoonUtility.GetGameSaveLocation(), htSaveTable.JsonString());
            }
            catch (Exception e)
            {
              NoonUtility.Log("Couldn't save game: " + e.Message);
            }

        }

        public void DeleteCurrentSave()
        {
            File.Delete(NoonUtility.GetGameSaveLocation());
        }

        //for saving from the tabletop
        public void SaveActiveGame(TabletopTokenContainer tabletop,Character character)
        {
            var allStacks = tabletop.GetElementStacksManager().GetStacks();
            var currentSituationControllers = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();
            var metaInfo = Registry.Retrieve<MetaInfo>();
            var allDecks = character.DeckInstances;

            var htSaveTable = dataExporter.GetSaveHashTable(metaInfo,character,allStacks,
                currentSituationControllers,allDecks);

            BackupSave();
            File.WriteAllText(NoonUtility.GetGameSaveLocation(), htSaveTable.JsonString());
        }

        public Hashtable RetrieveHashedSaveFromFile()
        {
            string importJson = File.ReadAllText(NoonUtility.GetGameSaveLocation());
            Hashtable htSave = SimpleJsonImporter.Import(importJson);
            return htSave;
        }


        public void ImportHashedSaveToState(TabletopTokenContainer tabletop, IGameEntityStorage storage, Hashtable htSave)
        {
            dataImporter.ImportSavedGameToState(tabletop, storage, htSave);
        }

        public SavedCrossSceneState RetrieveSavedCrossSceneState()
        {
            var htSave = RetrieveHashedSaveFromFile();
            return dataImporter.ImportCrossSceneState(htSave);

        }


        public bool SaveGameHasMatchingVersionNumber(VersionNumber currentVersionNumber)
        {
            var htSave = RetrieveHashedSaveFromFile();
            var htMetaInfo = htSave.GetHashtable(SaveConstants.SAVE_METAINFO);
            if (htMetaInfo == null)
                return false;
            if (!htMetaInfo.ContainsKey(SaveConstants.SAVE_VERSIONNUMBER))
                return false; //no version number

            string savedVersionString=htMetaInfo[SaveConstants.SAVE_VERSIONNUMBER].ToString();

            return currentVersionNumber.MajorVersionMatches(new VersionNumber(savedVersionString));
        }
    }
}
