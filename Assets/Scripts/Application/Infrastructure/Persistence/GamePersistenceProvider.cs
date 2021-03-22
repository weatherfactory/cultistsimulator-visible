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
using UnityEngine;


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

            var backupSaveTask = WriteSaveFile(GetBackupSaveFileLocation(), json);
            await backupSaveTask;


            var writeToFileTask = WriteSaveFile(saveFilePath, json);

            await writeToFileTask;

            NoonUtility.Log($"Saved game via {this.GetType().Name} at {DateTime.Now}",0, VerbosityLevel.Significants);

            return true;
        }

        private async Task WriteSaveFile(string saveFilePath, string JsonToSave)
        {
  


            var task = Task.Run(() =>
            {
               
                FileInfo fileInfo = new System.IO.FileInfo(saveFilePath);
                fileInfo.Directory.Create(); 
                File.WriteAllText(fileInfo.FullName, JsonToSave);
            });
            await task;
        }


        protected string GetBackupSaveFileLocation()
        {
            return $"{Application.persistentDataPath}/backups/save.json";
        }



        //copies old version in case of corruption
        private void BackupSave(int index)
        {
            const int MAX_BACKUPS = 5;
            // Back up a number of previous saves
            for (int i = MAX_BACKUPS - 1; i >= 1; i--)
            {
                if (File.Exists(GetBackupSaveGameLocation(i)))  //otherwise we can't copy it
                    File.Copy(GetBackupSaveGameLocation(i), GetBackupSaveGameLocation(i + 1), true);
            }
            // Back up the main save
            if (File.Exists(GetSaveFileLocation()))	//otherwise we can't copy it
                File.Copy(GetSaveFileLocation(), GetBackupSaveGameLocation(1), true);
        }

        private string GetBackupSaveGameLocation(int i)
        {
            return i.ToString();
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
