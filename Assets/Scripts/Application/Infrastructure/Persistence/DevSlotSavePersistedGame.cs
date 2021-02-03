using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecretHistories.Infrastructure.Persistence
{
    public class DevSlotSavePersistedGame: PersistedGame
    {
        private readonly int _slotNumber;

        public DevSlotSavePersistedGame(int slotNumber)
        {
            _slotNumber = slotNumber;
        }

        public override string GetSaveFileLocation()
        {
            return $"{Application.persistentDataPath}/devsave_{_slotNumber}.json";
        }

    }
}
