using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecretHistories.Infrastructure.Persistence
{
    public class DefaultPersistedGame: PersistedGame
    {
        public override string GetSaveFileLocation()
        {
            return $"{Application.persistentDataPath}/save.json";
        }


    }
}
