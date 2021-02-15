using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Commands;
using SecretHistories.Commands.Encausting;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;


namespace SecretHistories.Infrastructure.Persistence
{
    public abstract class GamePersistenceProvider
    {
        protected abstract string GetSaveFileLocation();


        protected PersistedGameState _persistedGameState;

        public PersistedGameState RetrievePersistedGameState()
        {
            return _persistedGameState;
        }

        public virtual void Encaust(IEnumerable<Character> characters, IEnumerable<Sphere> spheres)
        {
      
            _persistedGameState=new PersistedGameState();


            Encaustery<CharacterCreationCommand> characterEncaustery = new Encaustery<CharacterCreationCommand>();
            foreach (var character in characters)
            {
                var encaustedCharacterCommand = characterEncaustery.Encaust(character);
                _persistedGameState.CharacterCreationCommands.Add(encaustedCharacterCommand);
            }
            
            
            Encaustery<TokenCreationCommand> tokenEncaustery = new Encaustery<TokenCreationCommand>();

            foreach (var sphere in spheres)
            {
                var allTokensInSphere = sphere.GetAllTokens();
                foreach (var t in allTokensInSphere)
                {
                    var encaustedTokenCommand = tokenEncaustery.Encaust(t);
                    _persistedGameState.TokenCreationCommands.Add(encaustedTokenCommand);
                }
            }
        }



        public virtual bool Exists()
        {
            return File.Exists(GetSaveFileLocation());
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

        public virtual void ImportPetromnemeStateAfterTheAncientFashion()
        {
            //do nothing
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
