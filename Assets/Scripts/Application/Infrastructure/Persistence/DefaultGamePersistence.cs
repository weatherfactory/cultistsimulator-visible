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
using UnityEngine;

namespace SecretHistories.Infrastructure.Persistence
{
    public class DefaultGamePersistence: GamePersistence
    {
        public override string GetSaveFileLocation()
        {
            return $"{Application.persistentDataPath}/save.json";
        }


        public override void DeserialiseFromPersistence()
        {
            string json = File.ReadAllText(GetSaveFileLocation());
            var sh=new SerializationHelper();
            _persistedGameState = sh.DeserializeFromJsonString<PersistedGameState>(json);
        }
    }
}
