using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Commands;
using SecretHistories.Commands.Encausting;
using SecretHistories.Constants;
using SecretHistories.Enums;
using UnityEngine;

namespace SecretHistories.Infrastructure.Persistence
{
    public class DefaultGamePersistenceProvider: GamePersistenceProvider
    {
        protected override string GetSaveFileLocation()
        {
            return $"{persistentDataPath}/save.json";
        }

        public void PurgeSaveFileIrrevocably()
        {
            File.Delete(GetSaveFileLocation());
        }

    }
}
