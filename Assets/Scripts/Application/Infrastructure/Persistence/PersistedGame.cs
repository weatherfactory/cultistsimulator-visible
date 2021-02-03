using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Constants;


namespace SecretHistories.Infrastructure.Persistence
{
    public abstract class PersistedGame
    {
        public abstract string GetSaveFileLocation();

        public virtual bool Exists()
        {
            return File.Exists(GetSaveFileLocation());
        }

        public virtual CharacterCreationCommand GetCharacterCreationCommandFromSave()
        {
            var importer=new SimpleJSONGameDataImporter();
            return importer.ImportToCharacterCreationCommand(this);
        }

        public async Task<bool> SaveActiveGameAsync(string jsonToSave, PersistedGame source)
        {
            var saveFilePath = GetSaveFileLocation();

            var writeToFileTask = WriteSaveFile(saveFilePath, jsonToSave);

            await writeToFileTask;
            return true;
        }

        private async Task WriteSaveFile(string saveFilePath, string JsonToSave)
        {
            var task = Task.Run(() => File.WriteAllText(saveFilePath, JsonToSave));
            await task;
        }
    }
}
