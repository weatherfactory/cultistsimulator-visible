
using System.Collections.Generic;
using Assets.Scripts.Application.Commands.SituationCommands;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.UI;

namespace SecretHistories.Infrastructure.Persistence
{
   public class PetromnemeGamePersistence: GamePersistence
    {
        public override string GetSaveFileLocation()
        {
            return $"{UnityEngine.Application.persistentDataPath}/save.txt";
        }


        public override void DeserialiseFromPersistence()
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
    }
}
