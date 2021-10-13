
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.UI;

namespace SecretHistories.Infrastructure.Persistence
{
    /// <summary>
    /// Converter for classic CS SimpleJson save files
    /// </summary>
   public class PetromnemeGamePersistenceProvider: GamePersistenceProvider
   {
       private const string SAVE_FILE_NAME = "save.txt";

        protected override string GetSaveFileLocation()
        {
            return $"{UnityEngine.Application.persistentDataPath}/{SAVE_FILE_NAME}";
        }


        public override void DepersistGameState()
        {
            var importer = new PetromnemeImporter();
            _persistedGameState=new PersistedGameState();

            NoonUtility.Log("PETRO: Attempting to import character to CharacterCreationCommand.");
            var characterCreationCommand= importer.ImportToCharacterCreationCommand(this);
            _persistedGameState.CharacterCreationCommands.Add(characterCreationCommand);

            var rootPopulationCommand = importer.ImportTableState(this,characterCreationCommand.ActiveLegacy); //if we don't have an active legacy, we've no business importing table state
            _persistedGameState.RootPopulationCommand = rootPopulationCommand;

        }

        public Hashtable RetrieveHashedSaveFromFile()
        {

            string importJson = File.ReadAllText(GetSaveFileLocation());
            Hashtable htSave = SimpleJsonImporter.Import(importJson);
            return htSave;
        }

        public string TryRenameImportedSaveFile()
        {
            if (!File.Exists(GetSaveFileLocation()))
                return ($"PETRO: Can't find {GetSaveFileLocation()}, can't rename!");
            else
            {
                string newSaveFileName = $"save_txt_imported_{DateTime.Now:dd_MM_yyyy__hh_mm}.txt";
                newSaveFileName=newSaveFileName.Replace(" ", "_");
                string newSaveFileLocation = GetSaveFileLocation().Replace(SAVE_FILE_NAME, newSaveFileName);
                try
                {
                  
                    
                    File.Move(GetSaveFileLocation(), newSaveFileLocation);
                    return $"PETRO: Imported save.txt; renamed to {newSaveFileLocation}";
                }
                catch (Exception e)
                {
                    return ($"PETRO: Tried to rename {GetSaveFileLocation()} to  {newSaveFileLocation}  but ran into a problem: {e.Message}");
                }
            }
        }
    }
}
