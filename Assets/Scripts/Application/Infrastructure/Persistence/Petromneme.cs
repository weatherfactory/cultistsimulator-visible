
using System.Collections.Generic;
using SecretHistories.Commands;
using SecretHistories.Constants;

namespace SecretHistories.Infrastructure.Persistence
{
   public class Petromneme: PersistableGameState
    {
        public override string GetSaveFileLocation()
        {
            return $"{UnityEngine.Application.persistentDataPath}/save.txt";
        }


        public override void DeserialiseFromPersistence()
        {
            var importer = new SimpleJSONGameDataImporter();
            _characterCreationCommands=new List<CharacterCreationCommand>();
            _characterCreationCommands.Add(importer.ImportToCharacterCreationCommand(this));

        }
    }
}
