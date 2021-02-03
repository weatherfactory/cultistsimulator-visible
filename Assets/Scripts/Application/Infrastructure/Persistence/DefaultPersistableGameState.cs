using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Constants;
using UnityEngine;

namespace SecretHistories.Infrastructure.Persistence
{
    public class DefaultPersistableGameState: PersistableGameState
    {
        public override string GetSaveFileLocation()
        {
            return $"{Application.persistentDataPath}/save.json";
        }


        public override void DeserialiseFromPersistence()
        {
            throw new NotImplementedException();
        }
    }
}
