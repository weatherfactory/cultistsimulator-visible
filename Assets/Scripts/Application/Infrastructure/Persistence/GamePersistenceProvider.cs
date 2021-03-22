using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Commands.Encausting;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;


namespace SecretHistories.Infrastructure.Persistence
{
    public abstract class GamePersistenceProvider
    {
        protected abstract string GetSaveFileLocation();


        protected PersistedGameState _persistedGameState=new PersistedGameState();

        public PersistedGameState RetrievePersistedGameState()
        {
            return _persistedGameState;
        }

        public virtual void Encaust(Stable stable,HornedAxe hornedAxe)
        {

            var characters = stable.GetAllCharacters();
     


            _persistedGameState =new PersistedGameState();


            Encaustery<CharacterCreationCommand> characterEncaustery = new Encaustery<CharacterCreationCommand>();
            foreach (var character in characters)
            {
                var encaustedCharacterCommand = characterEncaustery.Encaust(character);
                _persistedGameState.CharacterCreationCommands.Add(encaustedCharacterCommand);
            }
            
            Encaustery<RootPopulationCommand> rootEncaustery = new Encaustery<RootPopulationCommand>();

     _persistedGameState.RootPopulationCommand=rootEncaustery.Encaust(FucineRoot.Get());
            
        }



        public virtual bool Exists()
        {
            return File.Exists(GetSaveFileLocation());
        }


        public virtual GameSpeed GetDefaultGameSpeed()
        {
            return GameSpeed.Paused;
        }

        public virtual void DepersistGameState()
        {
            string json = File.ReadAllText(GetSaveFileLocation());
            var sh = new SerializationHelper();
            _persistedGameState = sh.DeserializeFromJsonString<PersistedGameState>(json);
            _persistedGameState.NotificationCommands.Add(new AddNoteCommand(Watchman.Get<ILocStringProvider>().Get("UI_LOADEDTITLE"), 
                Watchman.Get<ILocStringProvider>().Get("UI_LOADEDDESC"),
                new Context(Context.ActionSource.Loading)));
        }


        public async Task<bool> SerialiseAndSaveAsync()
        {
            var serializationHelper=new SerializationHelper();

            var json = serializationHelper.SerializeToJsonString(_persistedGameState);

            var saveFilePath = GetSaveFileLocation();

            var writeToFileTask = WriteSaveFile(saveFilePath, json);

            await writeToFileTask;
            return true;
        }

        private async Task WriteSaveFile(string saveFilePath, string JsonToSave)
        {
            var task = Task.Run(() => File.WriteAllText(saveFilePath, JsonToSave));
            await task;
        }


        public static GamePersistenceProvider GetMostRelevantValidGamePersistence()
        {
            var defaultPersistence = new DefaultGamePersistenceProvider();
            if (defaultPersistence.Exists())
                return defaultPersistence;
            var petromnemePersistence = new PetromnemeGamePersistenceProvider();
            if (petromnemePersistence.Exists())
                return petromnemePersistence;

            var defaultLegacy = Watchman.Get<Compendium>().GetEntitiesAsList<Legacy>().First();


            return new FreshGameProvider(defaultLegacy);
        }

    }
}
