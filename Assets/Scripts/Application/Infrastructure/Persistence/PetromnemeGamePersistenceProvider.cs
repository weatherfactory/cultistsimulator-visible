
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
        protected override string GetSaveFileLocation()
        {
            return $"{UnityEngine.Application.persistentDataPath}/save.txt";
        }


        public override void DepersistGameState()
        {
            var importer = new SimpleJSONGameDataImporter();
            _persistedGameState=new PersistedGameState();

            var characterCreationCommand= importer.ImportToCharacterCreationCommand(this);
            _persistedGameState.CharacterCreationCommands.Add(characterCreationCommand);

            importer.ImportTableState(this, Watchman.Get<HornedAxe>().GetDefaultWorldSphere()); //this isn't running through the commands list!
        }

       
        public Hashtable RetrieveHashedSaveFromFile()
        {

            string importJson = File.ReadAllText(GetSaveFileLocation());
            Hashtable htSave = SimpleJsonImporter.Import(importJson);
            return htSave;
        }
    }
}
