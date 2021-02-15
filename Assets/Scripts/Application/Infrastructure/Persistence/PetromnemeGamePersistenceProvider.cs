
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
        }

        public override void ImportPetromnemeStateAfterTheAncientFashion()
        {
            var importer = new SimpleJSONGameDataImporter();
            importer.ImportTableState(this, Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());
        }

        public Hashtable RetrieveHashedSaveFromFile()
        {

            string importJson = File.ReadAllText(GetSaveFileLocation());
            Hashtable htSave = SimpleJsonImporter.Import(importJson);
            return htSave;
        }
    }
}
