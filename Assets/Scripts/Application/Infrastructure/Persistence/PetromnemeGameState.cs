
using System.Collections.Generic;
using Assets.Scripts.Application.Commands.SituationCommands;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.UI;

namespace SecretHistories.Infrastructure.Persistence
{
   public class PetromnemeGameState: PersistableGameState
    {
        public override string GetSaveFileLocation()
        {
            return $"{UnityEngine.Application.persistentDataPath}/save.txt";
        }


        public override void DeserialiseFromPersistence()
        {
            var importer = new SimpleJSONGameDataImporter();
            _characterCreationCommands = new List<CharacterCreationCommand>
            {
                importer.ImportToCharacterCreationCommand(this)
            };

            _tokenCreationCommands=new List<TokenCreationCommand>();

        }

        public override void ImportPetromnemeStateAfterTheAncientFashion()
        {
            var importer = new SimpleJSONGameDataImporter();
            importer.ImportTableState(this, Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());

        }
    }
}
