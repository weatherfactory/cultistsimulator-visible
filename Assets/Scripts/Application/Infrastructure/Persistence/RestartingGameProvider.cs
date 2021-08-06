using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Infrastructure.Persistence;
using UnityEngine;

namespace SecretHistories.Infrastructure.Persistence

{
    public class RestartingGameProvider: GamePersistenceProvider
    {
        protected override string GetBackupSaveFileLocation()
        {
            return null; //restarts never get a backup
        }
        protected override string GetSaveFileLocation()
        {
            return $"{Application.persistentDataPath}/restart.json";
        }

    }
}
