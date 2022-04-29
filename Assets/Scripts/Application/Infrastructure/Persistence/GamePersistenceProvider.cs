using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Commands.Encausting;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Commands.TokenEffectCommands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEditor;
using UnityEngine;


namespace SecretHistories.Infrastructure.Persistence
{
    public abstract class GamePersistenceProvider
    {



        protected abstract string GetSaveFileLocation();


        protected PersistedGameState _persistedGameState=new PersistedGameState();


        /// <summary>
        /// get the game state we've retrieved. One day we might add validation checks. Who knows?
        /// </summary>
        /// <returns></returns>
        public PersistedGameState RetrievePersistedGameState()
        {
            return _persistedGameState;
        }

        /// <summary>
        /// Supply a game state we've created arbitrarily or rescued from the moon or something
        /// </summary>
        /// <param name="gameState"></param>
        public void SupplyGameState(PersistedGameState gameState)
        {
            if(_persistedGameState.CharacterCreationCommands.Any() || _persistedGameState.NotificationCommands.Any() || _persistedGameState.RootPopulationCommand.Spheres.Any())
                throw new ApplicationException("'The sun turns black; earth sinks in the sea.'. Supplying a gamestate to a gamestateprovider with a non-empty game state isn't allowed.");

            _persistedGameState = gameState;
        }

        public virtual void Encaust(Stable stable,FucineRoot fucineRoot,Xamanek xamanek)
        {

            var characters = stable.GetAllCharacters();
     
            _persistedGameState =new PersistedGameState();
            _persistedGameState.Version = Watchman.Get<MetaInfo>().VersionNumber;

            Encaustery<CharacterCreationCommand> characterEncaustery = new Encaustery<CharacterCreationCommand>();
            foreach (var character in characters)
            {
                var encaustedCharacterCommand = characterEncaustery.Encaust(character);
                _persistedGameState.CharacterCreationCommands.Add(encaustedCharacterCommand);
            }
            
            Encaustery<RootPopulationCommand> rootEncaustery = new Encaustery<RootPopulationCommand>();
            _persistedGameState.RootPopulationCommand=rootEncaustery.Encaust(fucineRoot);

            Encaustery<PopulateXamanekCommand> xamanekEncaustery = new Encaustery<PopulateXamanekCommand>();
            _persistedGameState.PopulateXamanekCommand = xamanekEncaustery.Encaust(xamanek);

        }


        public virtual bool IsValid()
        {
            return true;
        }
        public virtual bool SaveExists()
        {
            return File.Exists(GetSaveFileLocation());
        }


        public virtual GameSpeed GetDefaultGameSpeed()
        {
            return GameSpeed.Paused;
        }

        public virtual void DepersistGameState()
        {
            NoonUtility.Log($"LOADING: Depersisting game state from {GetSaveFileLocation()}");

            string json = File.ReadAllText(GetSaveFileLocation());
            var sh = new SerializationHelper();
            _persistedGameState = sh.DeserializeFromJsonString<PersistedGameState>(json);
            NoonUtility.Log($"LOADING: Deserialized game state from JSON");

            var note = new Notification(Watchman.Get<ILocStringProvider>().Get("UI_LOADEDTITLE"),
                Watchman.Get<ILocStringProvider>().Get("UI_LOADEDDESC"));

            _persistedGameState.NotificationCommands.Add(new AddNoteToTokenCommand(note,
                new Context(Context.ActionSource.Loading)));
        }


        public async Task<bool> SerialiseAndSaveAsync()
        {
            var serializationHelper=new SerializationHelper();

            var json = serializationHelper.SerializeToJsonString(_persistedGameState);

            var saveFilePath = GetSaveFileLocation();

            var writeToFileTask = WriteSaveFile(saveFilePath, json);
            await writeToFileTask;

            NoonUtility.Log($"Saved game via {this.GetType().Name} to {saveFilePath} at {DateTime.Now}",0, VerbosityLevel.Significants);

            var backupSaveFileLocation = GetBackupSaveFileLocation();
            if(!string.IsNullOrEmpty(backupSaveFileLocation))
            {
                var backupSaveTask = WriteSaveFile(backupSaveFileLocation, json);
                await backupSaveTask;
                NoonUtility.Log($"Saved backup via {this.GetType().Name} to {backupSaveFileLocation} at {DateTime.Now}", 0, VerbosityLevel.Significants);

                var purgeBackupsTask = PurgeBackups();
                await purgeBackupsTask;
            }


            return true;
        }

        private async Task WriteSaveFile(string saveFilePath, string JsonToSave)
        {
        var task = Task.Run(() =>
            {
               
                FileInfo fileInfo = new FileInfo(saveFilePath);
                fileInfo.Directory.Create(); 
                File.WriteAllText(fileInfo.FullName, JsonToSave);
            });
            await task;
        }


        protected virtual string GetBackupSaveFileLocation()
        {
            string persistentDataPath = Watchman.Get<MetaInfo>().PersistentDataPath;

            return $"{persistentDataPath}/backups/save{DateTime.Now:yyyyMMddHHmmssfff}.json";
        }

        protected async Task PurgeBackups()
        {
            const int MAX_BACKUPS = 7;

            var task = Task.Run(() =>
            {
                FileInfo backupsFileInfo = new FileInfo(GetBackupSaveFileLocation());
                if (backupsFileInfo == null)
                    return;
                var existingFiles = backupsFileInfo.Directory.GetFiles().OrderBy(f => f.CreationTime);
                int existingFilesCount = existingFiles.Count();

                if (existingFilesCount > MAX_BACKUPS)
                {
                    var filesToPurge = existingFiles.Take(existingFilesCount - MAX_BACKUPS);
                    foreach (var f in filesToPurge)
                        f.Delete();

                }
            });

            await task;

        }

        
        public static GamePersistenceProvider GetBestGuessGamePersistence()
        {
            var defaultPersistence = new DefaultGamePersistenceProvider();
            if (defaultPersistence.SaveExists())
                return defaultPersistence;
            //var petromnemePersistence = new PetromnemeGamePersistenceProvider();
            //if (petromnemePersistence.Exists())
            //    return petromnemePersistence;

            var defaultLegacy = Watchman.Get<Compendium>().GetEntitiesAsList<Legacy>().First();


            return new FreshGameProvider(defaultLegacy);
        }

    }
}
