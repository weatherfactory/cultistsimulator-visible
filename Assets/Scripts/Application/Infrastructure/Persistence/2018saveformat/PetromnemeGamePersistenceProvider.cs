
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.UI;

namespace SecretHistories.Infrastructure.Persistence
{
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

            var characterCreationCommand= importer.ImportToCharacterCreationCommand(this);
            _persistedGameState.CharacterCreationCommands.Add(characterCreationCommand);

            importer.ImportTableState(this, Watchman.Get<HornedAxe>().GetDefaultSphere()); //this isn't running through the commands list!
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
                return ($"Can't find {GetSaveFileLocation()}, can't rename!");
            else
            {
                string newSaveFileName = $"save_txt_imported_{DateTime.Now:dddd_MM_yyyy__hh_mm}.txt";
                newSaveFileName=newSaveFileName.Replace(" ", "_");
                string newSaveFileLocation = GetSaveFileLocation().Replace(SAVE_FILE_NAME, newSaveFileName);
                try
                {
                  
                    
                    File.Move(GetSaveFileLocation(), newSaveFileLocation);
                    return $"Imported save.txt; renamed to {newSaveFileLocation}";
                }
                catch (Exception e)
                {
                    return ($"Tried to rename {GetSaveFileLocation()} to  {newSaveFileLocation}  but ran into a problem: {e.Message}");
                }
            }
        }
    }
}
